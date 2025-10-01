using System;
using System.Collections.Generic;
using TechC.Utility;
using UnityEngine;

namespace TechC.Main.Stage
{
    /// <summary>
    /// エンドレスランナー,ステージ管理
    /// ステージは床と壁がセットになったオブジェクトをランダムに置く側に配置していく
    /// </summary>
    public class StageManager : Singleton<StageManager>
    {
        [Header("ステージの出現設定")]
        [SerializeField] private StageTypeStageDataDictionary stageDataDict = new StageTypeStageDataDictionary();

        [SerializeField] private float zOffset = 30f; // ステージ1つの長さ
        [SerializeField] private int initStageCount = 10; // 最初に並べる数
        [SerializeField] private float spawnThreshold;
        [SerializeField] private float initOffset = 4.5f;
        [SerializeField] private Transform player; // 監視対象（プレイヤーやカメラなど）

        private Vector3 spawnPos; // 次に生成する座標
        private Queue<GameObject> existStageObjects = new Queue<GameObject>();

        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            spawnPos = Vector3.zero;
            spawnPos.z += initOffset;
        }
        private void Start()
        {
            // 最初にまとめて生成
            for (int i = 0; i < initStageCount; i++)
            {
                SpawnNextStage();
            }
        }

        private void Update()
        {
            if (player != null && existStageObjects.Count > 0)
            {
                // 最後に生成されたステージ
                var lastStage = existStageObjects.ToArray()[existStageObjects.Count - 1];

                // 最後のステージの後端位置
                Vector3 lastEndPos = lastStage.transform.position + new Vector3(0, 0, zOffset);

                // プレイヤーとの距離を計算
                float distance = Vector3.Distance(player.position, lastEndPos);

                // 一定距離以下なら生成
                if (distance < spawnThreshold)
                {
                    SpawnNextStage();

                    // 古いのを削除
                    if (existStageObjects.Count > initStageCount)
                    {
                        var old = existStageObjects.Dequeue();
                        StageFactory.I.ReturnObj(old);
                    }
                }
            }
        }


        /// <summary>
        /// ランダム抽選でステージ生成
        /// </summary>
        private void SpawnNextStage()
        {
            var data = GetRandomStage();
            if (data == null || data.stagePrefab == null)
            {
                Debug.LogError("StageManager: StageDataが無効です");
                return;
            }

            var stage = StageFactory.I.GetObj(data.stagePrefab, spawnPos, Quaternion.identity);
            existStageObjects.Enqueue(stage);

            // 次のスポーン位置を進める
            spawnPos.z += zOffset;
        }

        /// <summary>
        /// 確率に基づいてランダム選択
        /// </summary>
        public StageData GetRandomStage()
        {
            var dict = stageDataDict.ToDictionary();
            float total = 0f;
            foreach (var d in dict.Values) total += d.probability;

            if (total <= 0f) return null;

            float rand = UnityEngine.Random.value * total;
            float cumulative = 0f;

            foreach (var kvp in dict)
            {
                cumulative += kvp.Value.probability;
                if (rand <= cumulative)
                    return kvp.Value;
            }

            return null;
        }
    }

    [Serializable]
    public enum StageType
    {
        NormalArea_1,
        NormalArea_2,
        NormalArea_3,
        NormalArea_4
    }

    [Serializable]
    public class StageData
    {
        [Tooltip("ステージのプレハブ")]
        public GameObject stagePrefab;

        [Tooltip("出現確率（合計値に対する割合）")]
        [Range(0f, 1f)]
        public float probability = 1f; // デフォルト値を設定
    }
}