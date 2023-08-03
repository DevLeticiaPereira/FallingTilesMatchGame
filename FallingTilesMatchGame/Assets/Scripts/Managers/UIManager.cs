using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Managers
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private List<PanelInfo> _panelsInfo;
        [SerializeField] private Camera _uiCamera;

        private Dictionary<PanelType, GameObject> _panels = new Dictionary<PanelType, GameObject>();
        private Dictionary<PanelType, GameObject> _panelPrefabs = new Dictionary<PanelType, GameObject>();
        private RectTransform _rectTransform;
        private List<PanelType> _openedPanelsOrder = new List<PanelType>();

        public PanelType PanelOnFocus { get; private set; }

        public static event Action<PanelType> PanelFocusEntered;
        public static event Action<PanelType> PanelFocusExited;

        public enum PanelType
        {
            None,
            MainMenu,
            Hud,
            Pause,
            Settings,
            Confirm
        }

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();

            foreach (var panelInfo in _panelsInfo)
            {
                _panelPrefabs[panelInfo.Type] = panelInfo.prefab;
            }
        }

        public bool LoadPanel(PanelType panelType)
        {
            if (_panels.ContainsKey(panelType))
            {
                return false;
            }
            
            if (!_panelPrefabs.TryGetValue(panelType, out GameObject panelPrefab))
            {
                Debug.LogError($"Missing prefab for panel type {panelType}");
                return false;
            }
            
            var panel = Instantiate(panelPrefab, _rectTransform);
            if (!panel.TryGetComponent(out Panel panelComponent))
            {
                Destroy(panel);
                Debug.LogError("Missing Panel Component from instantiated panel prefab of type " + panelType);
                return false;
            }
            
            _panels[panelType] = panel;
            panelComponent.Initialize(panelType, _uiCamera);
            Utilities.AddUniqueToList(ref _openedPanelsOrder, panelType);
            SetFocusTo(panelType);
            return true;
        }

        public void UnloadPanel(PanelType panelType)
        {
            if (! _panels.TryGetValue(panelType, out GameObject panel))
            {
                return;
            }

            Destroy(panel);
            _panels.Remove(panelType);
            _openedPanelsOrder.Remove(panelType);
            
            if (panelType == PanelOnFocus)
            {
                // Set focus to the previous panel (if available) when closing the current one
                if (_openedPanelsOrder.Count > 0)
                {
                    int lastIndex = _openedPanelsOrder.Count - 1;
                    SetFocusTo(_openedPanelsOrder[lastIndex]);
                }
                else
                {
                    PanelOnFocus = PanelType.None;
                }
            }
        }

        public void UnloadAll()
        {
            if (_panels.Count == 0) return;
            
            foreach (var panelType in _panels)
            {
                Destroy(panelType.Value);
                _openedPanelsOrder.Remove(panelType.Key);
            }
            _panels.Clear();
        }
        
        public bool ShowConfirmPanel(string message, Action onConfirmAction, Action onCancelAction)
        {
            PanelType confirmPanelType = PanelType.Confirm;
            bool success = LoadPanel(confirmPanelType);
            if (!success) return false;
            
            ConfirmPanel confirmPanel = _panels[confirmPanelType].GetComponent<ConfirmPanel>();
            confirmPanel.Setup(message, onConfirmAction, onCancelAction);
            return true;
        }

        private void SetFocusTo(PanelType panelType)
        {
            PanelFocusExited?.Invoke(PanelOnFocus);
            PanelOnFocus = panelType;
            _panels[panelType].transform.SetAsLastSibling();
            PanelFocusEntered?.Invoke(PanelOnFocus);
        }

        [Serializable]
        public struct PanelInfo
        {
            public PanelType Type;
            public GameObject prefab;
        }
    }
}
