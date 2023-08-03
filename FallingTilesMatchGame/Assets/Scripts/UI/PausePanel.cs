using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace UI
{
    public class PausePanel : Panel
    {
        public void ExitGame()
        {
            GameManager.Instance.ExitGame();
        }

        public void ResumeGame()
        {
            GameManager.Instance.PauseGame(false);
        }
    }
}

