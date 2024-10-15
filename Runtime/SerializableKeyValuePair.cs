using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystemPackage {

    [Serializable]
    public struct SerializableKeyValuePair<TKey, TValue> {

        [SerializeField]
        private TKey key;

        [SerializeField]
        private TValue value;


        public SerializableKeyValuePair (TKey key, TValue value) {
            this.key = key;
            this.value = value;
        }


        public static implicit operator KeyValuePair<TKey, TValue> (SerializableKeyValuePair<TKey, TValue> item) {
            return new KeyValuePair<TKey, TValue>(item.key, item.value);
        }


        public static implicit operator SerializableKeyValuePair<TKey, TValue> (KeyValuePair<TKey, TValue> item) {
            return new SerializableKeyValuePair<TKey, TValue>(item.Key, item.Value);
        }


        public void Deconstruct (out TKey key, out TValue value) {
            key = this.key;
            value = this.value;
        }

    }

}