using Xunit;

namespace IdentityServer4.Contrib.DynamoDB.Tests
{
    [CollectionDefinition("grant")]
    public class CollectionFixture :
        ICollectionFixture<TestContext>
    {
    }
}