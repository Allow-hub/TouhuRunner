using System.Collections;
using TechC.Manager;
using TMPro;
using UnityEngine;

namespace TechC.Main
{
    /// <summary>
    /// Startのカウントダウンを担当
    /// </summary>
    public class StartView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private int countdownTime = 3;
        [SerializeField] private float speed = 0.7f;//実際のカウント速度

        private void Start()
        {
            // 初期状態
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            GameManager.I.IsPaused = true;

            // カウントダウン開始
            StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            int timeLeft = countdownTime;

            while (timeLeft > 0)
            {
                countdownText.text = timeLeft.ToString();
                yield return new WaitForSeconds(speed);
                timeLeft--;
            }

            // 最後に "GO!" を表示
            countdownText.text = "GO!";
            yield return new WaitForSeconds(speed);

            // UIを非表示にしてゲーム開始
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            GameManager.I.IsPaused = false;
        }
    }
}