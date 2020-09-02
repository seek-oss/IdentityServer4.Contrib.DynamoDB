using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using IdentityServer4.Stores;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using IdentityServer4.Contrib.DynamoDB.Stores;

namespace IdentityServer4.Contrib.DynamoDB.Tests
{
    [Collection("grant")]
    public class PersistedGrantTests
    {
        private readonly TestContext _;

        private readonly IPersistedGrantStore persistedGrantStore;

        public PersistedGrantTests(TestContext context)
        {
            this._ = context;
            var client = new AmazonDynamoDBClient(awsAccessKeyId: "fakeMyKeyId", awsSecretAccessKey: "fakeSecretAccessKey", new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" });
            this.persistedGrantStore = new PersistedGrantStore(client, new Extensions.DynamoDBOptions { DynamoDBContextConfig = new DynamoDBContextConfig { TableNamePrefix = "local-" } }, Mock.Of<ILogger<PersistedGrantStore>>());
        }

        [Fact]
        public async Task Store_Grant_And_Retrieve_It()
        {
            var key = Guid.NewGuid().ToString("N");

            var grant = new IdentityServer4.Models.PersistedGrant
            {
                Key = key,
                Expiration = DateTime.UtcNow.AddDays(1),
                Type = "test",
                ClientId = "client1",
                SubjectId = "sub1",
                SessionId = "session1",
                CreationTime = DateTime.UtcNow,
                Description = "des",
                Data = "bla bla",
                ConsumedTime = DateTime.UtcNow
            };

            await persistedGrantStore.StoreAsync(grant);

            var item = await persistedGrantStore.GetAsync(key);

            item.Should().NotBeNull();
            item.Key.Should().Be(key);
            item.Type.Should().Be(grant.Type);
            item.ClientId.Should().Be(grant.ClientId);
            item.SubjectId.Should().Be(grant.SubjectId);
            item.Data.Should().Be(grant.Data);
            item.Description.Should().Be(grant.Description);
            item.SessionId.Should().Be(grant.SessionId);
            item.CreationTime.Should().NotBe(new DateTime());
            item.Expiration.Should().NotBeNull();
            item.ConsumedTime.Should().NotBeNull();
        }

        [Fact]
        public async Task Store_Grant_And_Remove_It()
        {
            var key = Guid.NewGuid().ToString("N");

            var grant = new IdentityServer4.Models.PersistedGrant
            {
                Key = key,
                Expiration = DateTime.UtcNow.AddDays(1),
                Type = "test",
                ClientId = "client1",
                SubjectId = "sub1",
                SessionId = "session1",
                CreationTime = DateTime.UtcNow,
                Description = "des",
                Data = "bla bla",
                ConsumedTime = DateTime.UtcNow
            };

            await persistedGrantStore.StoreAsync(grant);

            await persistedGrantStore.RemoveAsync(key);

            var item = await persistedGrantStore.GetAsync(key);

            item.Should().BeNull();
        }

        [Fact]
        public async Task Store_Grants_And_Retrieve_them()
        {
            for (int i = 0; i < 5; i++)
            {
                var key = Guid.NewGuid().ToString("N");

                var grant = new IdentityServer4.Models.PersistedGrant
                {
                    Key = key,
                    Expiration = DateTime.UtcNow.AddDays(1),
                    Type = "test",
                    ClientId = $"client{i}",
                    SubjectId = "sub1",
                    SessionId = "session1",
                    CreationTime = DateTime.UtcNow,
                    Description = "des",
                    Data = "bla bla",
                    ConsumedTime = DateTime.UtcNow
                };

                await persistedGrantStore.StoreAsync(grant);
            }

            var items = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            });

            items.Should().HaveCount(5);

            items = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                ClientId = "client1"
            });

            items.Should().HaveCount(1);

            items = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                SessionId = "session1"
            });

            items.Should().HaveCount(5);

            items = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                Type = "test"
            });

            items.Should().HaveCount(5);
        }

        [Fact]
        public async Task Store_Grants_And_Remove_them()
        {
            for (int i = 0; i < 5; i++)
            {
                var key = Guid.NewGuid().ToString("N");

                var grant = new IdentityServer4.Models.PersistedGrant
                {
                    Key = key,
                    Expiration = DateTime.UtcNow.AddDays(1),
                    Type = "test",
                    ClientId = $"client{i}",
                    SubjectId = "sub1",
                    SessionId = "session1",
                    CreationTime = DateTime.UtcNow,
                    Description = "des",
                    Data = "bla bla",
                    ConsumedTime = DateTime.UtcNow
                };

                await persistedGrantStore.StoreAsync(grant);
            }

            await persistedGrantStore.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            });

            var items = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            });

            items.Should().BeEmpty();
        }
    }
}