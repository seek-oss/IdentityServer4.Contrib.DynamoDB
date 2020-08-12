# IdentityServer4.Contrib.DynamoDB

IdentityServer4.Contrib.DynamoDB is a persistence layer using [AWS DynamoDB](https://aws.amazon.com/dynamodb/) for operational data capability for Identity Server 4. Specifically, this store provides implementation for [IPersistedGrantStore](http://docs.identityserver.io/en/release/topics/deployment.html#operational-data).

## How to use

You need to install the [nuget package](https://www.nuget.org/packages/IdentityServer4.Contrib.DynamoDB)

then you can inject the operational store in the Identity Server 4 Configuration at startup:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAWSService<IAmazonDynamoDB>();
    ...
    services.AddIdentityServer()
    ...
    .AddOperationalStore(options =>
    {
        options.DynamoDBContextConfig = new DynamoDBContextConfig
        {
            TableNamePrefix = "Dev_",
            SkipVersionCheck = true,
            ConsistentRead = true,
            IgnoreNullValues = true
            ...
        };
    })
    ...
}
```

## the solution approach

the solution was approached based on how the [SQL Store](https://github.com/IdentityServer/IdentityServer4/tree/main/src/EntityFramework.Storage) storing the operational data, but the concept of DynamoDB as a NoSQL db is totally different than relational db concepts, all the operational data stores implement the following [IPersistedGrantStore](https://github.com/IdentityServer/IdentityServer4/blob/main/src/Storage/src/Stores/IPersistedGrantStore.cs) interface:

```csharp
public interface IPersistedGrantStore
{
    Task StoreAsync(PersistedGrant grant);

    Task<PersistedGrant> GetAsync(string key);

    Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter);

    Task RemoveAsync(string key);

    Task RemoveAllAsync(PersistedGrantFilter filter);
}
```

with the IPersistedGrantStore contract, we notice that the `GetAllAsync(filter)`, `RemoveAllAsync(filter)` defines a contract to read or remove all the grants in the store based on subject id, client ids and/or session ids and type of the grant, this is done in DynamoDB using the expensive `Scan` operation, if you are not happy with this implementation, you can override the implementation of `GetAllAsync` and `RemoveAllAsync` and use `Query` operation given that you already defined the right Indexes on DynamoDB table.

since DynamoDB has a [Document Expiration](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/TTL.html) feature based on a defined date time or time span, and to not implement a logic similar to SQL store implementation for [cleaning up the store](http://docs.identityserver.io/en/release/quickstarts/8_entity_framework.html) periodically from dangling grants, the store uses the ttl of DynamoDB while storing entries by setting `Expiration` value as linux epoch value.

> Note: setting the `Expiration` without explicitly enabling Ttl on DynamoDB table will not expire the documents.

## Feedback

feedbacks are always welcomed, please open an issue for any problem or bug found, and the suggestions are also welcomed.
