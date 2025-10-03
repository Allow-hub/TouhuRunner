using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.Manager
{
    /// <summary>
    /// フェードイン・アウトを管理するクラス
    /// </summary>
    public class FadeManager : Singleton<FadeManager>
    {
        [SerializeField] private Image fadeImage;  // 黒背景Image（Canvasに置く）
        [SerializeField] private float fadeDuration = 0.5f;
        
        protected override void Init()
        {
            base.Init();
        }

       public IEnumerator FadeOut()
        {
            if (fadeImage == null) yield break;
            float time = 0;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Clamp01(time / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(1f);
        }

        public IEnumerator FadeIn()
        {
            if (fadeImage == null) yield break;
            float time = 0;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(time / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(0f);
        }

        private void SetAlpha(float alpha)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
}