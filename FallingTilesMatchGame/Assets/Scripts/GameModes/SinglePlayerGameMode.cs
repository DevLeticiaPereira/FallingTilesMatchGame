using System;

namespace GameModes
{
    public class SinglePlayerGameMode : IGameMode
    {
        public int SinglePlayerHighScore { get; private set; }
        private GameManager _gameManager;
        private bool _playerMadeHighScore;
        private bool _gameIsOver;
        bool IGameMode.IsGameOver => _gameIsOver;
        
        public SinglePlayerGameMode(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void GameStart()
        {
            SinglePlayerHighScore = SaveManager.Instance.LoadSaveData().SingleModeHighScore;
            EventManager.InvokeEventSinglePlayerGameMode();
        }

        public string GetGameEndMessage()
        {
            string finalGameMessage;
            var singlePlayerHighScore = SinglePlayerHighScore;
            var playerScore = _gameManager.GridScoreMap[_gameManager.PlayerGrid.Value];
            if (_playerMadeHighScore)
            {
                finalGameMessage = _gameManager.GameSettings.SinglePlayerHighScoreMessage;
                finalGameMessage += playerScore;
            }
            else
            {
                string template = _gameManager.GameSettings.SinglePlayerStandardMessage;
                finalGameMessage = string.Format(template, playerScore, singlePlayerHighScore);
            }

            return finalGameMessage;
        }

        public void HandleGridGameOver(Guid GridID)
        {
            if (_gameManager.PlayerGrid == GridID)
            {
                _gameIsOver = true;
            }
        }

        public void HandleScore(int newScore)
        {
            if (newScore > SinglePlayerHighScore)
            {
                SinglePlayerHighScore = newScore;
                SaveManager.Instance.SaveSingleModeHighScore(newScore);
                EventManager.InvokeEventUpdateHighScore(newScore);
                _playerMadeHighScore = true;
            }
        }
    }
}