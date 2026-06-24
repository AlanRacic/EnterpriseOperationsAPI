using EnterpriseOperations.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface IExternalSystemService
    {
        Task<ExternalSystemStatusDto> GetStatusAsync();
    }
}
