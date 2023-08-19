using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModes
{
    public class MultiPlayerGameMode : IGameMode
    {
        private GameManager _gameManager;
        private List<Guid> _activeGrids;
        private bool _gameIsOver = false;
        private bool _playerWon = false;
        
        bool IGameMode.IsGameOver => _gameIsOver;
        
        public MultiPlayerGameMode(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        public void GameStart()
        {
            _activeGrids = _gameManager.GridScoreMap.Keys.ToList();
            EventManager.InvokeEventMultiPlayerGameMode();
        }
        
        public string GetGameEndMessage()
        {
            var finalGameMessage = _playerWon
                ? _gameManager.GameSettings.EndGameWinMessage
                : _gameManager.GameSettings.EndGameLostMessage;
            return finalGameMessage;
        }
        
        public void HandleGridGameOver(Guid gridID)
        {
            _activeGrids.Remove(gridID);
            if (gridID == _gameManager.PlayerGrid)
            {
                _gameIsOver = true;
                _playerWon = false;
                return;
            }
            if (_activeGrids.Count == 1)
            {
                _playerWon = true;
                _gameIsOver = true;
            }
        }
        
        public void HandleScore(int newScore) { }
    }
}