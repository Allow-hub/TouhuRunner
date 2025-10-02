using Unity.Services.LevelPlay;
using UnityEngine;

namespace TechC.Manager
{
    public class BannerAdManager : Singleton<BannerAdManager>
    {
        private LevelPlayBannerAd bannerTop;
        private LevelPlayBannerAd bannerBottom;

        // iOS用広告ユニットID
        [SerializeField] string iosAdUnitIdTop = "lxvduyxxcbynb5de";
        [SerializeField] string iosAdUnitIdBottom = "ioeuwr671ho6isr8";

        void Start()
        {
#if UNITY_IOS
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
#else
            Debug.LogWarning("このプラットフォームではバナー広告はサポートされていません");
#endif
        }

        private void OnDestroy()
        {
            bannerTop?.DestroyAd();
            bannerBottom?.DestroyAd();
        }
    }
}