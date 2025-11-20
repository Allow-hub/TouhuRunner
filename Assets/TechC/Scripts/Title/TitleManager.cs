using TechC.Manager;
using TMPro;
using Unity.Services.Authentication;
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
        
        [Header("Player Name")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button nameChangeButton;
        
        private const string PLAYER_NAME_KEY = "PlayerName";
        private const int MAX_NAME_LENGTH = 5;
        
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
            
            // 名前関連の初期化
            InitializePlayerName();
            nameChangeButton.onClick.AddListener(OnNameChanged);
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            
            // リアルタイム入力制限を追加
            nameInputField.onValueChanged.AddListener(OnNameInputValueChanged);
            nameInputField.contentType = TMP_InputField.ContentType.Alphanumeric; // 英数字のみ
        }

        /// <summary>
        /// 入力値がリアルタイムで変更された時（ローマ字以外を弾く）
        /// </summary>
        private void OnNameInputValueChanged(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            
            // ローマ字（英字）と数字のみを許可
            string filtered = FilterToAlphanumeric(input);
            
            // 変更があった場合のみ更新（無限ループ防止）
            if (filtered != input)
            {
                nameInputField.text = filtered;
                // カーソル位置を最後に移動
                nameInputField.caretPosition = filtered.Length;
            }
        }

        /// <summary>
        /// 英数字以外を除去
        /// </summary>
        private string FilterToAlphanumeric(string input)
        {
            string result = "";
            foreach (char c in input)
            {
                // a-z, A-Z, 0-9 のみ許可
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    result += c;
                }
            }
            return result;
        }

        /// <summary>
        /// プレイヤー名の初期化
        /// </summary>
        private async void InitializePlayerName()
        {
            // 既存の名前があればロード、なければランダム生成
            string playerName;
            if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
            {
                playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
            }
            else
            {
                playerName = GenerateRandomName();
                PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
                PlayerPrefs.Save();
                Debug.Log($"Generated new player name: {playerName}");
            }

            // InputFieldに表示
            nameInputField.text = playerName;

            // Unity Services初期化を待つ
            await WaitForAuthentication();
            
            // Unity Servicesに名前を設定
            await SetPlayerNameToUnityServices(playerName);
        }

        /// <summary>
        /// 認証完了を待つ
        /// </summary>
        private async System.Threading.Tasks.Task WaitForAuthentication()
        {
            int maxWait = 50; // 5秒
            while (!GameManager.I.IsUnityServicesInitialized && maxWait > 0)
            {
                await System.Threading.Tasks.Task.Delay(100);
                maxWait--;
            }
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("Authentication not completed within timeout");
            }
        }

        /// <summary>
        /// Unity Servicesに名前を設定
        /// </summary>
        private async System.Threading.Tasks.Task SetPlayerNameToUnityServices(string name)
        {
            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
                    Debug.Log($"Player name set to: {name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set player name: {e.Message}");
            }
        }

        /// <summary>
        /// ランダムな名前を生成
        /// </summary>
        private string GenerateRandomName()
        {
            string[] adjectives = { "Swift", "Brave", "Lucky", "Cool", "Super" };
            string[] nouns = { "Tofu", "Hero", "Star", "King", "Pro" };
            
            string adj = adjectives[Random.Range(0, adjectives.Length)];
            string noun = nouns[Random.Range(0, nouns.Length)];
            
            return $"{adj}{noun}".Substring(0, Mathf.Min(MAX_NAME_LENGTH, (adj + noun).Length));
        }

        /// <summary>
        /// 名前変更ボタンが押された時
        /// </summary>
        private async void OnNameChanged()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            
            string newName = nameInputField.text.Trim();
            
            // 空白チェック
            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogWarning("Name cannot be empty!");
                nameInputField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY);
                return;
            }

            // 念のため英数字チェック
            newName = FilterToAlphanumeric(newName);

            // 5文字に切り詰め
            if (newName.Length > MAX_NAME_LENGTH)
            {
                newName = newName.Substring(0, MAX_NAME_LENGTH);
            }

            nameInputField.text = newName;

            // 保存
            PlayerPrefs.SetString(PLAYER_NAME_KEY, newName);
            PlayerPrefs.Save();

            // Unity Servicesに反映
            await SetPlayerNameToUnityServices(newName);
            
            Debug.Log($"Player name changed to: {newName}");
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
            AudioManager.I.SetMasterVolume(value);
        }
    }
}