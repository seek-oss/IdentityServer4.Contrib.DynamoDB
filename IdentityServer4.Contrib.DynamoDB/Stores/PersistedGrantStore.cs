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
            try
            {
                var _grant = Models.Mapper.ToPersistedGrantModel(grant);

                using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
                {
                    await dBContext.SaveAsync(_grant);
                }
                logger.LogInformation("grant for subject {subjectId}, clientId {clientId}, grantType {grantType} and sessionId {session} persisted successfully", grant.SubjectId, grant.ClientId, grant.Type, grant.SessionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception storing persisted grant to DynamoDB database for subject {subjectId}, clientId {clientId}, grantType {grantType} and session {sessionId}", grant.SubjectId, grant.ClientId, grant.Type, grant.SessionId);
                throw;
            }
        }

        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            try
            {
                using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
                {
                    var grant = await dBContext.LoadAsync<Models.PersistedGrant>(key);

                    logger.LogInformation("{key} found in database: {hasValue}", key, grant != null);

                    if (grant is null)
                        return null;

                    var result = Models.Mapper.ToPersistedGrant(grant);

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception retrieving grant for key {key}", key);
                throw;
            }
        }

        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            try
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

                    logger.LogInformation("{grantsCount} persisted grants found for {subjectId}", result.Count(), filter.SubjectId);
                    return result.Select(Models.Mapper.ToPersistedGrant);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception while retrieving grants");
                throw;
            }
        }

        public virtual async Task RemoveAsync(string key)
        {
            try
            {
                using (var dBContext = new DynamoDBContext(dynamoDBClient, dynamoDBOptions.DynamoDBContextConfig))
                {
                    logger.LogInformation("removing {key} persisted grant from DynamoDB database", key);
                    await dBContext.DeleteAsync<Models.PersistedGrant>(key);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception removing {key} persisted grant from database", key);
                throw;
            }
        }

        public virtual async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            try
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
                    logger.LogInformation("removing {grantKeysCount} persisted grants from DynamoDB database for subject {subjectId}, clientId {clientId}, grantType {type} and session {session}", result.Count(), filter.SubjectId, filter.ClientId, filter.Type, filter.SessionId);

                    foreach (var grant in result)
                        await dBContext.DeleteAsync<Models.PersistedGrant>(grant);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception removing persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {type} and session {session}", filter.SubjectId, filter.ClientId, filter.Type, filter.SessionId);
                throw;
            }
        }
    }
}