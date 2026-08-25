namespace EnterpriseOperations.Application.Settings
{
    public class CacheSettings
    {
        public string Provider { get; set; } = "Memory";
        public int ExternalSystemStatusExpirationMinutes { get; set; }
        public int OperationTasksPagedExpirationMinutes { get; set; }
    }
}
