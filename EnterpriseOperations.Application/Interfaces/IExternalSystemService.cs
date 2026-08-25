using EnterpriseOperations.Application.DTOs;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IExternalSystemService
    {
        Task<ExternalSystemStatusDto> GetStatusAsync();
    }
}
