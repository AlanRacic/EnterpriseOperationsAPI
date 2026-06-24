using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.ExternalServices
{
    public class ExternalSystemService : IExternalSystemService
    {
        private readonly HttpClient _httpClient;

        public ExternalSystemService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public async Task<ExternalSystemStatusDto> GetStatusAsync()
        {
            var response = await _httpClient.GetAsync("/status");

            response.EnsureSuccessStatusCode();

            return new ExternalSystemStatusDto
            {
                SystemName = "External Operations System",
                Status = "Available",
                CheckedAt = DateTime.UtcNow,
                Source = "External API"
            };
        }
    }
}
