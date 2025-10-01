using UnityEngine;

namespace TechC.Main.Stage
{
    /// <summary>
    /// ステージを管理するファクトリー
    /// </summary>
    public class StageFactory : Singleton<StageFactory>
    {
        [SerializeField] private ObjectPool objectPool;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// オブジェクトプールからオブジェクトを取得
        /// </summary>
        /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
        /// <returns></returns>
        public GameObject GetObj(GameObject prefab) => objectPool.GetObject(prefab);
        public GameObject GetObj(GameObject prefab, Vector3 pos, Quaternion rot) => objectPool.GetObject(prefab, pos, rot);
        public void ReturnObj(GameObject obj) => objectPool.ReturnObject(obj);
    }
}
