using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        Task RemoveAsync(string key);

        Task<int> GetVersionAsync(string key);

        Task IncrementVersionAsync(string key);
    }
}
