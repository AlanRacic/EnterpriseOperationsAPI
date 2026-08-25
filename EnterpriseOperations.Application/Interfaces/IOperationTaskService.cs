using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Models;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IOperationTaskService
    {
        Task<IEnumerable<OperationTaskDto>> GetAllAsync();

        Task<PagedResult<OperationTaskDto>> GetPagedAsync(OperationTaskQueryParameters queryParameters);

        Task<OperationTaskDto?> GetByIdAsync(int id);

        Task<OperationTaskDto> CreateAsync(CreateOperationTaskDto dto);

        Task<bool> UpdateAsync(int id, UpdateOperationTaskDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
