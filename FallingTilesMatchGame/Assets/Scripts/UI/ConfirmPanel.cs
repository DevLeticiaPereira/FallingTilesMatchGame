using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ConfirmPanel : Panel
    {
        [SerializeField] private TMP_Text _confirmMessage;
        private Action _onConfirmAction;
        private Action _onCancelAction;

        public void Setup(string message, Action onConfirmAction, Action onCancelAction)
        {
            _confirmMessage.text = message;
            _onConfirmAction = onConfirmAction;
            _onCancelAction = onCancelAction;
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