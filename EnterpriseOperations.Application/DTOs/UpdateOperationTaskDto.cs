using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.DTOs
{
    public class UpdateOperationTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
