using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TechC.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private float maxMoveDistance;//今までの最高移動距離
        private float maxScore;//今までの最高スコア
        private int currentMoney;//スコア/xをお金として加算、所持金

        private float moveDistance;//進んだ距離
        public float MoveDistance => moveDistance;

        private float score;//壁ぎりぎりでの避けで加算
        public float Score => score;
        [SerializeField] private int targetFrameRate = 144;
        [HideInInspector] public float DeltaTime { get; private set; }
        public bool IsPaused = false;
        
        // Unity Services初期化完了フラグ
        public bool IsUnityServicesInitialized { get; private set; }
        
        // プレイヤー名関連
        private const string PLAYER_NAME_KEY = "PlayerName";
        private const string PROFILE_KEY = "CurrentProfile";
        public const int MAX_NAME_LENGTH = 5;
        
        public enum GameState
        {
            Title,
            Menu,
            Game,
            Result
        }
        public GameState currentState = GameState.Title;
        
        protected override void Init()
        {
            base.Init();

            AudioManager.I.PlayBGM(BGMID.Game);
            // VSyncCount を Dont Sync に変更
            QualitySettings.vSyncCount = 0;
            // fps 144 を目標に設定
            Application.targetFrameRate = targetFrameRate;
            
            // Unity Services初期化（async/awaitだが呼び捨て）
            InitializeUnityServicesAsync();
        }

        /// <summary>
        /// Unity Services初期化（デバッグ用：複数アカウント対応）
        /// </summary>
        private async void InitializeUnityServicesAsync()
        {
            try
            {
                // プロファイルを選択（デバッグ用）
                #if UNITY_EDITOR
                string profile = PlayerPrefs.GetString(PROFILE_KEY, "default");
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(profile));
                Debug.Log($"Unity Services initialized with profile: {profile}");
                
                // デバッグ用：Shiftキーで強制新規アカウント
                bool isShiftPressed = false;
                #if ENABLE_INPUT_SYSTEM
                isShiftPressed = Keyboard.current != null && 
                                (Keyboard.current.leftShiftKey.isPressed || 
                                 Keyboard.current.rightShiftKey.isPressed);
                #else
                isShiftPressed = Input.GetKey(KeyCode.LeftShift);
                #endif
                
                if (isShiftPressed)
                {
                    AuthenticationService.Instance.ClearSessionToken();
                    Debug.Log("Session token cleared - will create new account");
                }
                #else
                await UnityServices.InitializeAsync();
                #endif

                // 匿名サインイン
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"Signed in as Player ID: {AuthenticationService.Instance.PlayerId}");
                }

                // ローカルに保存された名前をUnity Authenticationに反映
                string savedName = GetPlayerName();
                if (savedName != "Guest")
                {
                    await UpdatePlayerNameInAuthenticationAsync(savedName);
                }

                IsUnityServicesInitialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
                IsUnityServicesInitialized = false;
            }
        }

        /// <summary>
        /// プレイヤー名を取得（ローカルから）
        /// </summary>
        public string GetPlayerName()
        {
            return PlayerPrefs.GetString(PLAYER_NAME_KEY, "Guest");
        }

        /// <summary>
        /// プレイヤー名を保存（ローカル + Unity Authentication）
        /// </summary>
        public async Task<bool> SavePlayerNameAsync(string name)
        {
            // 5文字に切り詰め
            if (name.Length > MAX_NAME_LENGTH)
                name = name.Substring(0, MAX_NAME_LENGTH);
            
            // ローカルに保存
            PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
            PlayerPrefs.Save();
            
            // Unity Authenticationにも反映
            return await UpdatePlayerNameInAuthenticationAsync(name);
        }

        /// <summary>
        /// Unity Authenticationにプレイヤー名を設定（内部用）
        /// </summary>
        private async Task<bool> UpdatePlayerNameInAuthenticationAsync(string name)
        {
            if (!IsUnityServicesInitialized)
            {
                Debug.LogWarning("Unity Services not initialized yet");
                return false;
            }

            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
                    Debug.Log($"✓ Player name updated in Authentication: {name}");
                    return true;
                }
                else
                {
                    Debug.LogWarning("Not signed in to Authentication Service");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Failed to update player name in Authentication: {e.Message}");
                return false;
            }
        }

        // プロファイル切り替え用メソッド（エディタ専用）
        #if UNITY_EDITOR
        [ContextMenu("Switch to Profile 1")]
        private void SwitchToProfile1() => SwitchProfile("profile1");

        [ContextMenu("Switch to Profile 2")]
        private void SwitchToProfile2() => SwitchProfile("profile2");

        [ContextMenu("Switch to Profile 3")]
        private void SwitchToProfile3() => SwitchProfile("profile3");

        [ContextMenu("Switch to Default Profile")]
        private void SwitchToDefaultProfile() => SwitchProfile("default");

        private void SwitchProfile(string profileName)
        {
            PlayerPrefs.SetString(PROFILE_KEY, profileName);
            PlayerPrefs.Save();
            Debug.Log($"Profile switched to: {profileName}. Restart the game!");
            UnityEditor.EditorApplication.isPlaying = false;
        }
        #endif

        private void Update()
        {
            DeltaTime = IsPaused ? 0f : Time.deltaTime;
            StateHandler();
        }

        private void SetState(GameState state)
        {
            currentState = state;
            switch (state)
            {
                case GameState.Title:
                    break;
                case GameState.Game:
                    moveDistance = 0;
                    score = 0;
                    break;
            }
        }

        private void StateHandler()
        {
            //switch (currentState)
            //{

            //}
        }

        private void ChangeCursorMode(bool visible, CursorLockMode cursorLockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = cursorLockMode;
        }

        // 非同期でシーンをロード
        public void LoadSceneAsync(int sceneIndex)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        // 非同期でシーンをロードするコルーチン
        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            // フェードアウト
            yield return StartCoroutine(FadeManager.I.FadeOut());

            // シーンロード
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncOperation.isDone)
                yield return null;

            // フェードイン
            yield return StartCoroutine(FadeManager.I.FadeIn());
        }

        //小規模なのでGameManagerが管理
        public void AddScore(float value) => score += value;
        public void AddDisrance(float value) => moveDistance += value;
        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeGameState() => SetState(GameState.Game);
        public void ChangeResultState() => SetState(GameState.Result);
    }
}