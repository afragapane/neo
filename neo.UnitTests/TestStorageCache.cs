using Neo.IO;
using Neo.IO.Caching;
using Neo.IO.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Neo.Ledger;

namespace Neo.UnitTests
{
    public class TestStorageCache<TKey, TValue> : DataCache<TKey, TValue>
        where TKey : IEquatable<TKey>, ISerializable
        where TValue : class, ICloneable<TValue>, ISerializable, new() 
    {
        public readonly Dictionary<TKey, TValue> values;


        public TestStorageCache()
        {
            values = new Dictionary<TKey, TValue>();
        }

        public TestStorageCache(Dictionary<TKey, TValue> values)
        {
            this.values = values;
        }
        public override void DeleteInternal(TKey key)
        {
            // Console.WriteLine(this.values);
            this.values.Remove(key);
        }

        public new void Commit() {
        }

        protected override void AddInternal(TKey key, TValue value)
        {
            // Console.WriteLine(this.values);
            // Console.WriteLine(this.dictionary.Count);
            this.values.Add(key, value);
        }

        protected override IEnumerable<KeyValuePair<TKey, TValue>> FindInternal(byte[] key_prefix)
        {
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>();
        }

        protected override TValue GetInternal(TKey key)
        {
            // Console.WriteLine("GETTING INTERNAL");
            // Console.WriteLine(this.dictionary.Count);
            if (!this.values.ContainsKey(key)) throw new NotImplementedException();
            return this.values[key];
        }

        protected override TValue TryGetInternal(TKey key)
        {
            // Console.WriteLine("TRYGETTING INTERNAL");
            // Console.WriteLine(this.dictionary.Count);
            // if (key is StorageKey) {
            //     var storageKey = (StorageKey)key;
            //     Console.WriteLine(storageKey.Key.ToHexString());
            // }
            if (this.values.ContainsKey(key)) return this.values[key];
            return null;
        }

        protected override void UpdateInternal(TKey key, TValue value)
        {
            // Console.WriteLine(this.values);
            // Console.WriteLine(this.dictionary.Count);
            this.values[key] = value;
        }
    }
}
