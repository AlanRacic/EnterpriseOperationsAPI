using EnterpriseOperations.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IOperationTaskService
    {
        Task<IEnumerable<OperationTaskDto>> GetAllAsync();

        Task<OperationTaskDto?> GetByIdAsync(int id);

        Task<OperationTaskDto> CreateAsync(CreateOperationTaskDto dto);
    }
}
