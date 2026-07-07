using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.ExternalServices
{
    public class ExternalSystemService : IExternalSystemService
    {
        private const string ExternalSystemStatusCacheKey = "external-system:status:last-known-good";

        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public ExternalSystemService(HttpClient httpClient, ICacheService cacheService, IOptions<CacheSettings> cacheOptions) 
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            _cacheSettings = cacheOptions.Value;
        }

        public async Task<ExternalSystemStatusDto> GetStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/200?sleep=500");

                response.EnsureSuccessStatusCode();

                var status = new ExternalSystemStatusDto
                {
                    SystemName = "External Operations System",
                    Status = "Available",
                    CheckedAt = DateTime.UtcNow,
                    Source = "External API"
                };

                await _cacheService.SetAsync(
                    ExternalSystemStatusCacheKey,
                    status,
                    TimeSpan.FromMinutes(_cacheSettings.ExternalSystemStatusExpirationMinutes));

                return status;
            }
            catch 
            {
                var cachedStatus = await _cacheService.GetAsync<ExternalSystemStatusDto>(ExternalSystemStatusCacheKey);

                if (cachedStatus is not null) 
                {
                    cachedStatus.Source = "Cache fallback";

                    return cachedStatus;
                }

                return new ExternalSystemStatusDto
                {
                    SystemName = "External Operations System",
                    Status = "Unavailable",
                    CheckedAt = DateTime.UtcNow,
                    Source = "Fallback response"
                };
            }
        }
    }
}
