using EnterpriseOperations.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.BackgroundJobs
{
    public class ExternalSystemStatusJob
    {
        private readonly IExternalSystemService _externalSystemService;
        private readonly ILogger<ExternalSystemStatusJob> _logger;

        public ExternalSystemStatusJob(IExternalSystemService externalSystemService, ILogger<ExternalSystemStatusJob> logger)
        {
            _externalSystemService = externalSystemService;
            _logger = logger;
        }

        public async Task CheckStatusAsync() 
        {
            var status = await _externalSystemService.GetStatusAsync();

            _logger.LogInformation(
                "External system status checked. SystemName: {SystemName}, Status: {Status}, Source: {Source}, CheckedAt: {CheckedAt}",
                status.SystemName,
                status.Status,
                status.Source,
                status.CheckedAt);
        }
    }
}
