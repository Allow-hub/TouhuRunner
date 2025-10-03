using System.Collections;
using System.Collections.Generic;
using TechC.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.Title
{
    public class TitleManager : Singleton<TitleManager>
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button menuButton;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
        }

        private void Start()
        {
            startButton.onClick.AddListener(StartGame);
            menuButton.onClick.AddListener(OnMenu);
        }

        private void StartGame()
        {
            GameManager.I.LoadSceneAsync(1);
            GameManager.I.ChangeGameState();
        }

        private void OnMenu()
        {
            
        }
    }
}
