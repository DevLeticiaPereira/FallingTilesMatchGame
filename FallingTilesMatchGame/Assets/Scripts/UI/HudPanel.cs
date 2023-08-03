using System;
using TMPro;
using UnityEngine;
using Managers;

namespace UI
{
    public class HudPanel : Panel
    {
        [SerializeField] private GameObject _endGameWindow;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _highScoreText;


        public void ShowEndGameWindow(bool active)
        {
            _endGameWindow.SetActive(active);
        }
    
        public void LoadMainMenu()
        {
            GameManager.Instance.ExitGame();
        }
    
        public void LoadPause()
        {
            GameManager.Instance.PauseGame(true);
        }

        private void OnScoreChanged(int newScore)
        {
            _scoreText.text = newScore.ToString();
        }
    
        private void OnHighScoreChanged(int highScore)
        {
            _highScoreText.text = highScore.ToString();
        }
    }
}

