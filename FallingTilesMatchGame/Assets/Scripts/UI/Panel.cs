using System;
using UnityEngine;
using Managers;
using UnityEngine.UI;

namespace UI
{
    public class Panel : MonoBehaviour
    {
        private GraphicRaycaster _graphicRaycaster;
        protected UIManager.PanelType _type;
    
        protected virtual void OnEnable()
        {
            UIManager.PanelFocusEntered += OnPanelFocusEnter;
            UIManager.PanelFocusExited += OnPanelFocusExit;
    
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (_graphicRaycaster == null)
            {
                _graphicRaycaster = this.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        protected void OnDisable()
        {
            UIManager.PanelFocusEntered -= OnPanelFocusEnter;
            UIManager.PanelFocusExited -= OnPanelFocusExit;
        }
        
        public void Initialize(UIManager.PanelType type)
        {
            _type = type;
        }
    
        public void Unload()
        {
            UIManager.Instance.UnloadPanel(_type);
        }
    
        protected virtual void OnPanelFocusExit(UIManager.PanelType panelType)
        {
            if (_type != panelType)
            {
                return;
            }
            _graphicRaycaster.enabled = false;
        }
    
        protected virtual void OnPanelFocusEnter(UIManager.PanelType panelType)
        {
            if (_type != panelType)
            {
                return;
            }
            _graphicRaycaster.enabled = true;
        }
    }
}

