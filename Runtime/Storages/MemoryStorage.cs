﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Storages {

    public class MemoryStorage : IDataStorage {

        private readonly Dictionary<string, byte[]> m_cache = new();
        private readonly Dictionary<string, DateTime> m_accessTimes = new();
        private readonly SortedDictionary<DateTime, string> m_sortedAccessTimes = new();
        private readonly long m_cacheCapacity;

        private long FreeSpace => m_cacheCapacity - m_cacheSize;
        private long m_cacheSize;


        public MemoryStorage (long cacheCapacity = SaveSystemConstants.FileSystemCacheSize) {
            m_cacheCapacity = cacheCapacity;
        }


        public Task Write ([NotNull] string key, byte[] data, CancellationToken token = default) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (data == null || data.Length == 0)
                return Task.CompletedTask;

            token.ThrowIfCancellationRequested();
            if (m_cache.TryGetValue(key, out byte[] value))
                m_cacheSize -= value.Length;
            if (FreeSpace <= 0)
                RemoveOldestRequestedData();

            m_cache[key] = data;
            m_cacheSize += data.Length;
            UpdateAccessTime(key);
            return Task.CompletedTask;
        }


        public Task<byte[]> Read ([NotNull] string key, CancellationToken token = default) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();
            if (!m_cache.ContainsKey(key))
                throw new KeyNotFoundException("Requested key doesn't exist in memory storage");
            UpdateAccessTime(key);
            return Task.FromResult(m_cache[key]);
        }


        public Task Delete ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (m_cache.TryGetValue(key, out byte[] value))
                m_cacheSize -= value.Length;
            m_cache.Remove(key);

            if (m_accessTimes.TryGetValue(key, out DateTime time))
                m_sortedAccessTimes.Remove(time);
            m_accessTimes.Remove(key);
            return Task.CompletedTask;
        }


        public Task Clear () {
            m_cacheSize = 0;
            m_cache.Clear();
            m_accessTimes.Clear();
            m_sortedAccessTimes.Clear();
            return Task.CompletedTask;
        }


        public Task<bool> Exists ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            return Task.FromResult(m_cache.ContainsKey(key));
        }


        private void UpdateAccessTime (string key) {
            if (m_accessTimes.TryGetValue(key, out DateTime time))
                m_sortedAccessTimes.Remove(time);

            DateTime now = DateTime.Now;
            m_accessTimes[key] = now;
            m_sortedAccessTimes[now] = key;
        }


        private void RemoveOldestRequestedData () {
            if (m_sortedAccessTimes.Count == 0)
                throw new InvalidOperationException("Nothing to remove");

            (DateTime time, string key) = m_sortedAccessTimes.First();
            m_cacheSize -= m_cache[key].Length;
            m_cache.Remove(key);
            m_accessTimes.Remove(key);
            m_sortedAccessTimes.Remove(time);
        }

    }

}