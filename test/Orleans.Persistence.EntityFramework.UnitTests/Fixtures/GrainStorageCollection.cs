using Xunit;

namespace Orleans.Persistence.EntityFramework.UnitTests.Fixtures
{
    [CollectionDefinition(Name)]
    public class GrainStorageCollection : ICollectionFixture<GrainStorageFixture>
    {
        public const string Name = nameof(GrainStorageCollection);
    }
}