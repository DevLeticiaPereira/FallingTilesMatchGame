using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConfirmPanel : Panel
    {
        [SerializeField] private TMP_Text _confirmMessage;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        private Action _onCancelAction;
        private Action _onConfirmAction;

        public void Setup(string message, Action onConfirmAction, Action onCancelAction)
        {
            _confirmMessage.text = message;
            _onCancelAction = onCancelAction;
            _onConfirmAction = onConfirmAction;
            _cancelButton.gameObject.SetActive(onCancelAction != null);
        }

        public void Confirm()
        {
            _onConfirmAction?.Invoke();
            Unload();
        }

        public void Cancel()
        {
            _onCancelAction?.Invoke();
            Unload();
        }
    }
}