using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechC.Manager;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
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
        
        [Header("Leaderboard UI")]
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private TextMeshProUGUI leaderboardTitleText; // "ランキング" タイトル用
        
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
        public async void ShowResult()
        {
            // Unity Services初期化待機
            await WaitForInitialization();

            try
            {
                // スコアをLeaderboardに送信（名前はAuthenticationから自動取得される）
                int moveDistance = Mathf.RoundToInt(GameManager.I.MoveDistance);
                
                // シンプルにスコアのみ送信（PlayerNameはAuthenticationから自動的に取得される）
                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    "TofuRanking", 
                    moveDistance
                );
                
                Debug.Log($"✓ Score submitted: {moveDistance}m");

                // ランキングを取得して表示
                await LoadAndDisplayLeaderboard();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to submit score to leaderboard: {e.Message}");
            }

            // リザルト表示
            distanceText.text = GameManager.I.MoveDistance.ToString("F0") + "m";
            scoreText.text = GameManager.I.Score.ToString("F0") + "にがりん";
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            GameManager.I.IsPaused = true;
        }

        /// <summary>
        /// Unity Services初期化を待機
        /// </summary>
        private async Task WaitForInitialization()
        {
            int maxWait = 50; // 5秒
            while (!GameManager.I.IsUnityServicesInitialized && maxWait > 0)
            {
                await Task.Delay(100);
                maxWait--;
            }

            if (!GameManager.I.IsUnityServicesInitialized)
            {
                Debug.LogWarning("Unity Services initialization timeout!");
            }
        }

        /// <summary>
        /// ランキングを取得して表示
        /// </summary>
        private async Task LoadAndDisplayLeaderboard()
        {
            try
            {
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                    "TofuRanking",
                    new GetScoresOptions { Limit = 10 }
                );

                Debug.Log($"=== Leaderboard Top 10 ===");
                for (int i = 0; i < scoresResponse.Results.Count; i++)
                {
                    var entry = scoresResponse.Results[i];
                    string playerName = CleanPlayerName(entry.PlayerName);
                    Debug.Log($"{i + 1}位: {entry.Score:F0}m - {playerName}");
                }

                if (leaderboardContent != null && leaderboardEntryPrefab != null)
                {
                    DisplayLeaderboardUI(scoresResponse.Results);
                }
                else
                {
                    Debug.LogWarning("Leaderboard UI is not set up. Displaying in console only.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load leaderboard: {e.Message}");
            }
        }

        /// <summary>
        /// プレイヤー名をクリーンアップ（#以降を削除、5文字に切り詰め）
        /// </summary>
        private string CleanPlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Guest";
            
            // #以降を削除（Unity AuthenticationのデフォルトフォーマットはPlayer#1234形式）
            int hashIndex = name.IndexOf('#');
            if (hashIndex >= 0)
            {
                name = name.Substring(0, hashIndex);
            }

            // 空になった場合
            if (string.IsNullOrEmpty(name))
                return "Guest";
            
            // 5文字に切り詰め
            return name.Length > GameManager.MAX_NAME_LENGTH 
                ? name.Substring(0, GameManager.MAX_NAME_LENGTH) 
                : name;
        }

        /// <summary>
        /// ランキングをUIに表示（上から下へ羅列）
        /// </summary>
        private void DisplayLeaderboardUI(List<LeaderboardEntry> entries)
        {
            // 既存の項目を削除
            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }

            // ランキングタイトル（オプション）
            if (leaderboardTitleText != null)
            {
                leaderboardTitleText.text = "=== ランキング TOP 10 ===";
            }

            // 上から順に表示
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                CreateLeaderboardEntry(i + 1, entry);
            }
            
            Debug.Log($"Displayed {entries.Count} leaderboard entries");
        }

        /// <summary>
        /// ランキング項目を生成
        /// </summary>
        private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
        {
            GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
            
            // RectTransformを取得してレイアウト設定
            RectTransform rectTransform = entryObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }

            Transform rankTransform = entryObj.transform.Find("RankText");
            Transform playerTransform = entryObj.transform.Find("PlayerText");
            Transform scoreTransform = entryObj.transform.Find("ScoreText");
            Image bg = entryObj.transform.Find("Image").gameObject.GetComponent<Image>();

            if (rankTransform != null)
            {
                var rankText = rankTransform.GetComponent<TextMeshProUGUI>();
                if (rankText != null) 
                {
                    rankText.text = $"{rank}";
                    // トップ3は色を変える（オプション）
                    if (rank == 1) rankText.color = new Color(1f, 0.84f, 0f); // ゴールド
                    else if (rank == 2) rankText.color = new Color(0.75f, 0.75f, 0.75f); // シルバー
                    else if (rank == 3) rankText.color = new Color(0.8f, 0.5f, 0.2f); // ブロンズ
                }
            }

            if (playerTransform != null)
            {
                var playerText = playerTransform.GetComponent<TextMeshProUGUI>();
                if (playerText != null)
                {
                    // entry.PlayerNameから直接取得（UpdatePlayerNameAsyncで設定された名前）
                    string playerName = CleanPlayerName(entry.PlayerName);
                    playerText.text = playerName;
                }
            }

            if (scoreTransform != null)
            {
                var scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (scoreText != null) scoreText.text = $"{entry.Score:F0}m";
            }

            // 自分のスコアをハイライト
            if (bg != null && entry.PlayerId == Unity.Services.Authentication.AuthenticationService.Instance.PlayerId)
            {
                bg.color = new Color(1f, 1f, 0.5f, 0.3f);
            }
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