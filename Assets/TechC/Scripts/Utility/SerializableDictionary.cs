using System;
using System.Collections.Generic;
using TechC.Main.Stage;
using UnityEngine;

namespace TechC.Utility
{
    /// <summary>
    /// Dictionaryをシリアル化
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Entry
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        private Dictionary<TKey, TValue> dict;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            dict = new Dictionary<TKey, TValue>();
            foreach (var e in entries)
            {
                // defaultキーも一応入れる（被りは無視 or 上書き）
                if (!dict.ContainsKey(e.key))
                    dict.Add(e.key, e.value);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            if (dict == null) OnAfterDeserialize();
            return dict;
        }
    }

    [Serializable]
    public class StageTypeStageDataDictionary : SerializableDictionary<StageType, StageData> { }
}
