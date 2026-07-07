using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Settings
{
    public class CacheSettings
    {
        public int ExternalSystemStatusExpirationMinutes { get; set; }
        public int OperationTasksPagedExpirationMinutes { get; set; }
    }
}
