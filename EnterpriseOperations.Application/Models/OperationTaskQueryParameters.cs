using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Models
{
    public class OperationTaskQueryParameters
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize 
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public bool? IsCompleted { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public string SortDirection { get; set; } = "desc";
    }
}
