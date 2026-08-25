using EnterpriseOperations.Application.Models;
using EnterpriseOperations.Domain.Entities;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IOperationTaskRepository
    {
        Task<IEnumerable<OperationTask>> GetAllAsync();

        Task<PagedResult<OperationTask>> GetPagedAsync(OperationTaskQueryParameters queryParameters);

        Task<OperationTask?> GetByIdAsync(int id);

        Task<OperationTask> AddAsync(OperationTask operationTask);

        Task<bool> UpdateAsync(OperationTask operationTask);

        Task<bool> DeleteAsync(int id);
    }
}
