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
        [SerializeField] private LayerMask tofuLayer;

        public static event Action OnGameOver;

        private bool hasTriggered = false;

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("A - 衝突検知");
            Debug.Log($"衝突したオブジェクト: {collision.gameObject.name}");
            Debug.Log($"衝突したオブジェクトのレイヤー: {collision.gameObject.layer}");
            Debug.Log($"設定されたTofuLayer: {tofuLayer.value}");
            
            if (hasTriggered) return;

            // 豆腐との衝突（ゲームオーバー）
            if ((tofuLayer.value & (1 << collision.gameObject.layer)) > 0)
            {
                Debug.Log("AA");
                HandleTofuCollision(collision.gameObject);
            }
            else
            {
                Debug.Log("レイヤーマスクの判定に失敗");
            }
        }

        /// <summary>
        /// 豆腐との衝突処理（ゲームオーバー）
        /// </summary>
        private void HandleTofuCollision(GameObject tofu)
        {
            hasTriggered = true;

            Debug.Log($"豆腐が壁に衝突: {tofu.name} -> {gameObject.name} (ゲームオーバー)");

            // ゲームオーバーイベント発火
            OnGameOver?.Invoke();

            // 豆腐を非アクティブ化または破壊
            tofu.SetActive(false);

        }

        private void OnDestroy()
        {
            // イベントのメモリリーク防止
            OnGameOver = null;
        }

        /// <summary>
        /// 壁をリセット（再利用時）
        /// </summary>
        public void ResetWall()
        {
            hasTriggered = false;
            gameObject.SetActive(true);
        }
    }
}
