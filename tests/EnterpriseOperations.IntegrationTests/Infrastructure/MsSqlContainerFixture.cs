using Testcontainers.MsSql;

namespace EnterpriseOperations.IntegrationTests.Infrastructure
{
    public sealed class MsSqlContainerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

        public string ConnectionString => _container.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}
