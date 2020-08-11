using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace IdentityServer4.Contrib.DynamoDB.Tests
{
    public class TableSetup
    {
        private const string tableName = "local-PersistedGrant";

        private static readonly IAmazonDynamoDB DynamoDBClient = new AmazonDynamoDBClient(awsAccessKeyId: "fakeMyKeyId", awsSecretAccessKey: "fakeSecretAccessKey", new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000"
        });

        public async Task CreateTable()
        {
            var request = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Key",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Key",
                        KeyType = "HASH"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                },
                TableName = tableName
            };


            var response = await DynamoDBClient.ListTablesAsync();
            if (response.TableNames.Count == 0)
            {
                await DynamoDBClient.CreateTableAsync(request);
                await WaitUntilTableActive(request.TableName);
            }
        }

        public async Task DeleteTable()
        {
            await DynamoDBClient.DeleteTableAsync(tableName);
        }

        private static async Task WaitUntilTableActive(string tableName)
        {
            string status = null;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    status = await GetTableStatus(tableName);
                }
                catch (ResourceNotFoundException)
                {
                    // DescribeTable is eventually consistent. So you might
                    // get resource not found. So we handle the potential exception.
                }

            } while (status != "ACTIVE");
        }

        private static async Task<string> GetTableStatus(string tableName)
        {
            var response = await DynamoDBClient.DescribeTableAsync(new DescribeTableRequest
            {
                TableName = tableName
            });

            return response.Table.TableStatus;
        }
    }

}