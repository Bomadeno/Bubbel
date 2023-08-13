using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Bubbel_Shot
{
 
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject inGameMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject mainMenuAndModeSelect;
        [SerializeField] private GameObject gameWonMenu;
        [SerializeField] private GameObject gameLostMenu;


        private Game game;

        //todo this isn't really very clean - e.g. EventBetter would make it cleaner 
        public void CrosslinkWithGame(Game _game)
        {
            this.game = _game;
            
            //set default menu states 
            DeactivateAllMenus();
            inGameMenu.SetActive(true);
        }

        private void DeactivateAllMenus()
        {
            inGameMenu.SetActive(false);
            pauseMenu.SetActive(false);
            mainMenuAndModeSelect.SetActive(false);
            gameWonMenu.SetActive(false);
            gameLostMenu.SetActive(false);
        }
        
        public void PauseGame()
        {
            inGameMenu.SetActive(false);
            pauseMenu.SetActive(true);
            game.PauseGame();
        }

        public void ResumeGame()
        {
            inGameMenu.SetActive(true);
            pauseMenu.SetActive(false);
            game.ResumeGame();
        }

        public void GetReadyToStart()
        {
            DeactivateAllMenus();
            inGameMenu.SetActive(true);
        }

        public void ExitGame()
        {
            game.ExitGame();
        }

        public void StartNewClassicGame()
        {
            game.StartNewClassicGame();
        }

        public void Won()
        {
            DeactivateAllMenus();
            
            gameWonMenu.SetActive(true);
        }

        public void Lost()
        {
            DeactivateAllMenus();
            
            gameLostMenu.SetActive(true);
        }
    }
   
}