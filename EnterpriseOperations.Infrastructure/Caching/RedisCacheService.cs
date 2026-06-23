using System.Text.Json;
using EnterpriseOperations.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer) 
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<T?> GetAsync<T>(string key) 
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(cachedValue)) 
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) 
        {
            var serializedValue = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options);
        }

        public async Task RemoveAsync(string key) 
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<int> GetVersionAsync(string key) 
        {
            var cachedVersion = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(cachedVersion)) 
            {
                await _distributedCache.SetStringAsync(key, "1");
                return 1;
            }

            return int.Parse(cachedVersion);
        }

        public async Task IncrementVersionAsync(string key) 
        {
            var database = _connectionMultiplexer.GetDatabase();

            await database.StringIncrementAsync(key);
        }
    }
}
