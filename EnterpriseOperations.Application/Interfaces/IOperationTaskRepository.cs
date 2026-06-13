using EnterpriseOperations.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IOperationTaskRepository
    {
        Task<IEnumerable<OperationTask>> GetAllAsync();

        Task<OperationTask?> GetByIdAsync(int id);

        Task<OperationTask> AddAsync(OperationTask operationTask);
    }
}
