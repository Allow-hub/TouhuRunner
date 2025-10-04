using TechC.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.Title
{
    public class TitleManager : Singleton<TitleManager>
    {
        [SerializeField] private Button menuClose;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Slider audioSlider;
        [SerializeField] private Button startButton;
        [SerializeField] private Button menuButton;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
        }

        private void Start()
        {
            menuPanel.SetActive(false);
            audioSlider.onValueChanged.AddListener(SetAudio);
            startButton.onClick.AddListener(StartGame);
            menuButton.onClick.AddListener(OnMenu);
            menuClose.onClick.AddListener(OnMenuClose);
        }

        private void StartGame()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            GameManager.I.LoadSceneAsync(1);
            GameManager.I.ChangeGameState();
        }

        private void OnMenu()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            menuPanel.SetActive(true);   
        }

        public void OnMenuClose()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            menuPanel.SetActive(false);
        }

        private void SetAudio(float value)
        {
            AudioManager.I.SetBGMVolume(value);
        }
    }
}
