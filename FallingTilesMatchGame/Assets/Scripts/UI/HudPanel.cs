using GameModes;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HudPanel : Panel
    {
        [SerializeField] private TMP_Text _highScoreText;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.EventUpdateHighScore += UpdateHighScore;
            EventManager.EventMultiPlayerGameMode += SetupMultiPlayerGameMode;
            EventManager.EventSinglePlayerGameMode += SetupSinglePlayerGameMode;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.EventUpdateHighScore -= UpdateHighScore;
            EventManager.EventMultiPlayerGameMode -= SetupMultiPlayerGameMode;
            EventManager.EventSinglePlayerGameMode -= SetupSinglePlayerGameMode;
        }
        
        private void SetupSinglePlayerGameMode()
        {
            _highScoreText.gameObject.SetActive(true);
            var singlePlayerGameMode = GameManager.Instance.GameMode as SinglePlayerGameMode;
            _highScoreText.text = singlePlayerGameMode.SinglePlayerHighScore.ToString();
        }

        private void SetupMultiPlayerGameMode()
        {
            _highScoreText.gameObject.SetActive(false);
        }

        private void UpdateHighScore(int highScore)
        {
            _highScoreText.text = highScore.ToString();
        }

        public void LoadMainMenu()
        {
            GameManager.Instance.ExitGame();
        }

        public void LoadPause()
        {
            GameManager.Instance.PauseGame(true);
        }
    }
}