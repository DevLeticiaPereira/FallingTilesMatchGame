using TMPro;
using UnityEngine;
using Managers;

namespace UI
{
	public class MainMenuPanel : Panel
	{
		[SerializeField] private GameObject _menuOptions;
		public void OpenGameSettings()
		{
			UIManager.Instance.LoadPanel( UIManager.PanelType.Settings);
		}

		protected override void OnPanelFocusEnter(UIManager.PanelType panelType)
		{
			base.OnPanelFocusEnter(panelType);
			if (panelType != _type)
			{
				return;
			}
			_menuOptions.SetActive(true);
		}
		
		protected override void OnPanelFocusExit(UIManager.PanelType panelType)
		{
			base.OnPanelFocusExit(panelType);
			if (panelType != _type)
			{
				return;
			}
			_menuOptions.SetActive(false);
		}

		public void StartGame(int numberOfPlayers)
		{
			GameManager.Instance.LoadGameScene(numberOfPlayers);
		}
	}
}
