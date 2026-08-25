namespace EnterpriseOperations.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<MsSqlContainerFixture>
{
    public const string Name = "IntegrationTests";
}
