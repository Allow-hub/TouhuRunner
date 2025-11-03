using Unity.Services.LevelPlay;
using UnityEngine;

namespace TechC.Manager
{
    // シングルトンとしてDontDestroyOnLoadで永続化
    public class LevelPlayInitializer : MonoBehaviour
    {
        private static LevelPlayInitializer instance;
        private static bool isInitialized = false;

        [SerializeField] private string iosAppKey = "23c9a0785";

        void Awake()
        {
            // シングルトンパターン
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // 初期化が済んでいない場合のみ実行
            if (!isInitialized)
            {
                InitializeLevelPlay();
            }
        }

        private void InitializeLevelPlay()
        {
#if UNITY_IOS
            Debug.Log("LevelPlay SDK 初期化開始");
            
            LevelPlay.OnInitSuccess += OnSdkInitSuccess;
            LevelPlay.OnInitFailed += OnSdkInitFailed;

            LevelPlay.Init(iosAppKey);
#else
            Debug.LogWarning("このプラットフォームではLevelPlayはサポートされていません");
#endif
        }

        private void OnSdkInitSuccess(LevelPlayConfiguration config)
        {
            Debug.Log("LevelPlay SDK 初期化成功");
            isInitialized = true;
        }

        private void OnSdkInitFailed(LevelPlayInitError error)
        {
            Debug.LogError($"LevelPlay SDK 初期化失敗: {error.ErrorMessage}");
            isInitialized = false;
        }

        public static bool IsInitialized()
        {
            return isInitialized;
        }

        private void OnDestroy()
        {
#if UNITY_IOS
            if (instance == this)
            {
                LevelPlay.OnInitSuccess -= OnSdkInitSuccess;
                LevelPlay.OnInitFailed -= OnSdkInitFailed;
            }
#endif
        }
    }
}