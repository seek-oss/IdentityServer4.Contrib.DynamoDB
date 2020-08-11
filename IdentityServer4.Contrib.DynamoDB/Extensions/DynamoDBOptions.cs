using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace IdentityServer4.Contrib.DynamoDB.Extensions
{
    public class DynamoDBOptions
    {
        public DynamoDBContextConfig DynamoDBContextConfig { get; set; }

        public DynamoDBOptions() { }
    }
}