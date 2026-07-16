using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Settings;
using EnterpriseOperations.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseOperations.Infrastructure.DependencyInjection
{
    public static class CacheServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheProvider(this IServiceCollection services, IConfiguration configuration) 
        {
            services.Configure<CacheSettings>(configuration.GetSection("Cache"));

            var cacheProvider = configuration["Cache:Provider"] ?? "Memory";

            if (string.Equals(cacheProvider, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redisConnectionString = configuration["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis was selected, but its connection string is missing.");

                services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else 
            {
                services.AddMemoryCache();

                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            return services;
        }
    }
}
