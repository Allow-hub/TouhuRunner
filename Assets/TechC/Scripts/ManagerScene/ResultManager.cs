using System.Collections;
using System.Collections.Generic;
using TechC.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.Main
{
    /// <summary>
    /// リザルトの管理クラス
    /// </summary>
    public class ResultManager : Singleton<ResultManager>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
            HideResult();
        }

        private void Start()
        {
            restartButton.onClick.AddListener(Restart);
            homeButton.onClick.AddListener(GoTitle);
        }

        /// <summary>
        /// リザルトを表示
        /// </summary>
        [ContextMenu("Show")]
        public void ShowResult()
        {
            distanceText.text = GameManager.I.MoveDistance.ToString("F0") + "m";
            scoreText.text = GameManager.I.Score.ToString("F0") + "にがりん";
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            GameManager.I.IsPaused = true;
        }

        private void HideResult()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void Restart()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            GameManager.I.LoadSceneAsync(1);
            GameManager.I.ChangeGameState();
        }
        private void GoTitle()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            GameManager.I.LoadSceneAsync(0);
            GameManager.I.ChangeTitleState();
        }
    }
}