using Unity.Services.LevelPlay;  // 追加済み
using UnityEngine;

namespace TechC.Manager
{
    public class BannerAdManager : Singleton<BannerAdManager>
    {
        private LevelPlayBannerAd bannerTop;
        private LevelPlayBannerAd bannerBottom;

        [SerializeField] string iosAdUnitIdTop = "lxvduyxxcbynb5de";
        [SerializeField] string iosAdUnitIdBottom = "ioeuwr671ho6isr8";

        [SerializeField] string iosAppKey = "23c9a0785";

        void Start()
        {
#if UNITY_IOS
            // SDK初期化イベントの登録
            LevelPlay.OnInitSuccess += OnSdkInitSuccess;
            LevelPlay.OnInitFailed += OnSdkInitFailed;

            // SDK初期化開始
            LevelPlay.Init(iosAppKey);
#else
            Debug.LogWarning("このプラットフォームではバナー広告はサポートされていません");
#endif
        }

        // SDK初期化成功時に広告をロード
        private void OnSdkInitSuccess(LevelPlayConfiguration config)
        {
            Debug.Log("LevelPlay SDK 初期化成功");

            // 上バナー
            if (!string.IsNullOrEmpty(iosAdUnitIdTop))
            {
                var configTop = new LevelPlayBannerAd.Config.Builder()
                    .SetSize(LevelPlayAdSize.BANNER)
                    .SetPosition(LevelPlayBannerPosition.TopCenter)
                    .SetDisplayOnLoad(true)
                    .SetRespectSafeArea(true)
                    .Build();

                bannerTop = new LevelPlayBannerAd(iosAdUnitIdTop, configTop);
                bannerTop.OnAdLoaded += (info) => Debug.Log("上バナー読み込み成功");
                bannerTop.OnAdLoadFailed += (error) => Debug.LogError("上バナー読み込み失敗: " + error.ErrorMessage);
                bannerTop.LoadAd();
            }

            // 下バナー
            if (!string.IsNullOrEmpty(iosAdUnitIdBottom))
            {
                var configBottom = new LevelPlayBannerAd.Config.Builder()
                    .SetSize(LevelPlayAdSize.BANNER)
                    .SetPosition(LevelPlayBannerPosition.BottomCenter)
                    .SetDisplayOnLoad(true)
                    .SetRespectSafeArea(true)
                    .Build();

                bannerBottom = new LevelPlayBannerAd(iosAdUnitIdBottom, configBottom);
                bannerBottom.OnAdLoaded += (info) => Debug.Log("下バナー読み込み成功");
                bannerBottom.OnAdLoadFailed += (error) => Debug.LogError("下バナー読み込み失敗: " + error.ErrorMessage);
                bannerBottom.LoadAd();
            }
        }

        private void OnSdkInitFailed(LevelPlayInitError error)
        {
            Debug.LogError("LevelPlay SDK 初期化失敗: " + error.ErrorMessage);
        }

        private void OnDestroy()
        {
#if UNITY_IOS
            LevelPlay.OnInitSuccess -= OnSdkInitSuccess;
            LevelPlay.OnInitFailed -= OnSdkInitFailed;
            bannerTop?.DestroyAd();
            bannerBottom?.DestroyAd();
#endif
        }
    }
}
