using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
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

        public ExternalSystemService(HttpClient httpClient, ICacheService cacheService) 
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
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
                    TimeSpan.FromMinutes(10));

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
