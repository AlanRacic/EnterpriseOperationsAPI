using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.DTOs
{
    public class ExternalSystemStatusDto
    {
        public string SystemName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public string Source { get; set; }
    }
}
