using Unity.Services.LevelPlay;
using UnityEngine;
using System.Collections;

namespace TechC.Manager
{
    // 各シーンに配置するバナー表示用マネージャー
    public class BannerAdManager : MonoBehaviour
    {
        private LevelPlayBannerAd bannerTop;
        private LevelPlayBannerAd bannerBottom;

        [SerializeField] private string iosAdUnitIdTop = "lxvduyxxcbynb5de";
        [SerializeField] private string iosAdUnitIdBottom = "ioeuwr671ho6isr8";

        [SerializeField] private bool showTopBanner = true;
        [SerializeField] private bool showBottomBanner = true;

        void Start()
        {
#if UNITY_IOS
            // SDK初期化を待ってから広告をロード
            StartCoroutine(WaitForInitAndLoadAds());
#else
            Debug.LogWarning("このプラットフォームではバナー広告はサポートされていません");
#endif
        }

        private IEnumerator WaitForInitAndLoadAds()
        {
            // SDK初期化完了を待つ(最大20秒)
            float timeout = 20f;
            float elapsed = 0f;

            while (!LevelPlayInitializer.IsInitialized() && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (!LevelPlayInitializer.IsInitialized())
            {
                Debug.LogError("LevelPlay SDK の初期化がタイムアウトしました");
                yield break;
            }

            // 初期化完了後に広告をロード
            LoadBanners();
        }

        private void LoadBanners()
        {
            // 上バナー
            if (showTopBanner && !string.IsNullOrEmpty(iosAdUnitIdTop))
            {
                try
                {
                    var configTop = new LevelPlayBannerAd.Config.Builder()
                        .SetSize(LevelPlayAdSize.BANNER)
                        .SetPosition(LevelPlayBannerPosition.TopCenter)
                        .SetDisplayOnLoad(true)
                        .SetRespectSafeArea(true)
                        .Build();

                    bannerTop = new LevelPlayBannerAd(iosAdUnitIdTop, configTop);
                    bannerTop.OnAdLoaded += OnTopBannerLoaded;
                    bannerTop.OnAdLoadFailed += OnTopBannerLoadFailed;
                    
                    bannerTop.LoadAd();
                    Debug.Log("上バナー読み込み開始");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"上バナー作成中にエラー: {e.Message}");
                }
            }

            // 下バナー
            if (showBottomBanner && !string.IsNullOrEmpty(iosAdUnitIdBottom))
            {
                try
                {
                    var configBottom = new LevelPlayBannerAd.Config.Builder()
                        .SetSize(LevelPlayAdSize.BANNER)
                        .SetPosition(LevelPlayBannerPosition.BottomCenter)
                        .SetDisplayOnLoad(true)
                        .SetRespectSafeArea(true)
                        .Build();

                    bannerBottom = new LevelPlayBannerAd(iosAdUnitIdBottom, configBottom);
                    bannerBottom.OnAdLoaded += OnBottomBannerLoaded;
                    bannerBottom.OnAdLoadFailed += OnBottomBannerLoadFailed;
                    
                    bannerBottom.LoadAd();
                    Debug.Log("下バナー読み込み開始");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"下バナー作成中にエラー: {e.Message}");
                }
            }
        }

        private void OnTopBannerLoaded(LevelPlayAdInfo info)
        {
            Debug.Log($"上バナー読み込み成功: {info.AdUnitId}");
        }

        private void OnTopBannerLoadFailed(LevelPlayAdError error)
        {
            Debug.LogError($"上バナー読み込み失敗: {error.ErrorMessage} (Code: {error.ErrorCode})");
        }

        private void OnBottomBannerLoaded(LevelPlayAdInfo info)
        {
            Debug.Log($"下バナー読み込み成功: {info.AdUnitId}");
        }

        private void OnBottomBannerLoadFailed(LevelPlayAdError error)
        {
            Debug.LogError($"下バナー読み込み失敗: {error.ErrorMessage} (Code: {error.ErrorCode})");
        }

        private void OnDestroy()
        {
#if UNITY_IOS
            if (bannerTop != null)
            {
                bannerTop.OnAdLoaded -= OnTopBannerLoaded;
                bannerTop.OnAdLoadFailed -= OnTopBannerLoadFailed;
                bannerTop.DestroyAd();
            }

            if (bannerBottom != null)
            {
                bannerBottom.OnAdLoaded -= OnBottomBannerLoaded;
                bannerBottom.OnAdLoadFailed -= OnBottomBannerLoadFailed;
                bannerBottom.DestroyAd();
            }
#endif
        }
    }
}