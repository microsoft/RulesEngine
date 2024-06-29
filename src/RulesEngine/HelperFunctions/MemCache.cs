// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RulesEngine.HelperFunctions
{
    public class MemCacheConfig {
        public int SizeLimit { get; set; } = 1000;
    }


    internal class MemCache
    {
        private readonly MemCacheConfig _config;
        private ConcurrentDictionary<string, (object value, DateTimeOffset expiry)> _cacheDictionary;
        private ConcurrentQueue<(string key, DateTimeOffset expiry)> _cacheEvictionQueue;

        public MemCache(MemCacheConfig config)
        {
            if(config == null)
            {
                config = new MemCacheConfig();
            }
            _config = config;
            _cacheDictionary = new ConcurrentDictionary<string, (object value, DateTimeOffset expiry)>();
            _cacheEvictionQueue = new ConcurrentQueue<(string key, DateTimeOffset expiry)>();
        }

        public bool TryGetValue<T>(string key,out T value)
        {
            value = default;
            if (_cacheDictionary.TryGetValue(key, out var cacheItem))
            {
                if(cacheItem.expiry < DateTimeOffset.UtcNow)
                {
                    _cacheDictionary.TryRemove(key, out _);
                    return false;
                }
                else
                {
                    value = (T)cacheItem.value;
                    return true;
                }   
            }
            return false;
           
        }


        public T Get<T>(string key)
        {
            TryGetValue<T>(key, out var value);
            return value;
        }


        /// <summary>
        /// Returns all known keys. May return keys for expired data as well
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            return _cacheDictionary.Keys;
        }

        public T GetOrCreate<T>(string key, Func<T> createFn, DateTimeOffset? expiry = null)
        {
            if(!TryGetValue<T>(key,out var value))
            {
                value = createFn();
                return Set<T>(key,value,expiry);
            }
            return value;
        }

        public T Set<T>(string key, T value, DateTimeOffset? expiry = null)
        {
            var fixedExpiry = expiry ?? DateTimeOffset.MaxValue;

            while (_cacheDictionary.Count > _config.SizeLimit)
            {
                if (_cacheEvictionQueue.IsEmpty)
                {
                    _cacheDictionary.Clear();
                }
                if(_cacheEvictionQueue.TryDequeue(out var result)
                    && _cacheDictionary.TryGetValue(result.key,out var dictionaryValue)
                    &&  dictionaryValue.expiry == result.expiry)
                {   
                    _cacheDictionary.TryRemove(result.key, out _);
                }
                
            }

            _cacheDictionary.AddOrUpdate(key, (value, fixedExpiry), (k, v) => (value, fixedExpiry));
            _cacheEvictionQueue.Enqueue((key, fixedExpiry));
            return value;
        }

        public void Remove(string key)
        {
            _cacheDictionary.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cacheDictionary.Clear();
            _cacheEvictionQueue =  new ConcurrentQueue<(string key, DateTimeOffset expiry)>();
        }
    }
}
