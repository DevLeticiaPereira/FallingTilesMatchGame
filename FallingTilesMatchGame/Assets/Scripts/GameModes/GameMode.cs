using System;
using System.Collections.Generic;
using System.Linq;

namespace GameModes
{
    public interface IGameMode
    {
        bool IsGameOver { get; }

        public void GameStart();
        public string GetGameEndMessage();
        public void HandleGridGameOver(Guid GridID);
        public void HandleScore(int newScore);
    }
}