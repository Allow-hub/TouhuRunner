using UnityEngine;
using System;

namespace TechC.MainScene.Wall
{
    /// <summary>
    /// 壁との衝突判定を管理するクラス
    /// </summary>
    public class WallCollider : MonoBehaviour
    {
        [Header("衝突判定設定")]
        [SerializeField] private LayerMask playerLayer;

        public static event Action OnGameOver;

        private bool hasTriggered = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (hasTriggered) return;

            // 豆腐との衝突（ゲームオーバー）
            if ((playerLayer.value & (1 << collision.gameObject.layer)) > 0)
            {
                HandlePlayerCollision(collision.gameObject);
            }
        }

        /// <summary>
        /// プレイヤーとの衝突処理
        /// </summary>
        private void HandlePlayerCollision(GameObject gameObject)
        {
            hasTriggered = true;

            // ゲームオーバーイベント発火
            OnGameOver?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
