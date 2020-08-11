using System;
using Amazon.Util;

namespace IdentityServer4.Contrib.DynamoDB.Models
{
    public static class Mapper
    {
        public static PersistedGrant ToPersistedGrantModel(IdentityServer4.Models.PersistedGrant grant)
        {
            return new PersistedGrant
            {
                Key = grant.Key,
                ClientId = grant.ClientId,
                SessionId = grant.SessionId,
                SubjectId = grant.SubjectId,
                Data = grant.Data,
                Type = grant.Type,
                Description = grant.Description,
                Expiration = grant.Expiration.HasValue ? new Nullable<int>(AWSSDKUtils.ConvertToUnixEpochSeconds(grant.Expiration.Value)) : null,
                CreationTime = grant.CreationTime,
                ConsumedTime = grant.ConsumedTime
            };
        }

        public static IdentityServer4.Models.PersistedGrant ToPersistedGrant(PersistedGrant grant)
        {
            return new IdentityServer4.Models.PersistedGrant
            {
                Key = grant.Key,
                ClientId = grant.ClientId,
                SessionId = grant.SessionId,
                SubjectId = grant.SubjectId,
                Data = grant.Data,
                Type = grant.Type,
                Description = grant.Description,
                Expiration = grant.Expiration.HasValue ? new DateTime?(AWSSDKUtils.ConvertFromUnixEpochSeconds(grant.Expiration.Value)) : null,
                CreationTime = grant.CreationTime,
                ConsumedTime = grant.ConsumedTime
            };
        }
    }
}