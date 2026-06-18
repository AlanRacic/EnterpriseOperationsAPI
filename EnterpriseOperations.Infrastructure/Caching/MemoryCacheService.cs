using EnterpriseOperations.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T? value);

            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);

            return Task.CompletedTask;
        }

        public Task<int> GetVersionAsync(string key) 
        {
            if (!_memoryCache.TryGetValue(key, out int version)) 
            {
                version = 1;
                _memoryCache.Set(key, version);
            }

            return Task.FromResult(version);
        }

        public Task IncrementVersionAsync(string key) 
        {
            var currentVersion = 1;

            if (_memoryCache.TryGetValue(key, out int existingVersion)) 
            {
                currentVersion = existingVersion;
            }

            _memoryCache.Set(key, currentVersion + 1);

            return Task.CompletedTask;
        }
    }
}
