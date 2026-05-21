using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Domain.Entities
{
    public class OperationTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
