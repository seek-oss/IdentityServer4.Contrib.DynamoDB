using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using IdentityServer4.Contrib.DynamoDB.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Contrib.DynamoDB.Stores
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        protected readonly IAmazonDynamoDB dynamoDBClient;
        protected readonly DynamoDBOptions dynamoDBOptions;
        protected readonly ILogger<PersistedGrantStore> logger;

        public PersistedGrantStore(IAmazonDynamoDB dynamoDBClient, DynamoDBOptions dynamoDBOptions, ILogger<PersistedGrantStore> logger)
        {
            this.dynamoDBClient = dynamoDBClient ?? throw new ArgumentNullException(nameof(dynamoDBClient));
            this.dynamoDBOptions = dynamoDBOptions ?? throw new ArgumentNullException(nameof(dynamoDBOptions));
            this.logger = logger;
        }

        public virtual async Task StoreAsync(PersistedGrant grant)
        {
            var _grant = Models.Mapper.ToPersistedGrantModel(grant);

            using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
            {
                await dBContext.SaveAsync(_grant);
            }
        }

        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
            {
                var grant = await dBContext.LoadAsync<Models.PersistedGrant>(key);

                if (grant is null)
                    return null;

                var result = Models.Mapper.ToPersistedGrant(grant);

                return result;
            }
        }

        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var conditions = new List<ScanCondition>();
            if (!string.IsNullOrEmpty(filter.SubjectId))
                conditions.Add(new ScanCondition(nameof(filter.SubjectId), ScanOperator.Equal, filter.SubjectId));
            if (!string.IsNullOrEmpty(filter.ClientId))
                conditions.Add(new ScanCondition(nameof(filter.ClientId), ScanOperator.Equal, filter.ClientId));
            if (!string.IsNullOrEmpty(filter.SessionId))
                conditions.Add(new ScanCondition(nameof(filter.SessionId), ScanOperator.Equal, filter.SessionId));
            if (!string.IsNullOrEmpty(filter.Type))
                conditions.Add(new ScanCondition(nameof(filter.Type), ScanOperator.Equal, filter.Type));

            using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
            {
                var result = await dBContext.ScanAsync<Models.PersistedGrant>(conditions).GetRemainingAsync();

                return result.Select(Models.Mapper.ToPersistedGrant);
            }
        }

        public virtual async Task RemoveAsync(string key)
        {
            using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
            {
                await dBContext.DeleteAsync<Models.PersistedGrant>(key);
            }
        }

        public virtual async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            var conditions = new List<ScanCondition>();
            if (!string.IsNullOrEmpty(filter.SubjectId))
                conditions.Add(new ScanCondition(nameof(filter.SubjectId), ScanOperator.Equal, filter.SubjectId));
            if (!string.IsNullOrEmpty(filter.ClientId))
                conditions.Add(new ScanCondition(nameof(filter.ClientId), ScanOperator.Equal, filter.ClientId));
            if (!string.IsNullOrEmpty(filter.SessionId))
                conditions.Add(new ScanCondition(nameof(filter.SessionId), ScanOperator.Equal, filter.SessionId));
            if (!string.IsNullOrEmpty(filter.Type))
                conditions.Add(new ScanCondition(nameof(filter.Type), ScanOperator.Equal, filter.Type));

            var data = new List<PersistedGrant>();
            using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
            {
                var result = await dBContext.ScanAsync<Models.PersistedGrant>(conditions).GetRemainingAsync();
                
                foreach(var grant in result)
                    await dBContext.DeleteAsync<Models.PersistedGrant>(grant);
            }
        }
    }
}