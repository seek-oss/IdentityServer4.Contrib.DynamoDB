using System;
using IdentityServer4.Contrib.DynamoDB.Extensions;
using IdentityServer4.Contrib.DynamoDB.Stores;
using IdentityServer4.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerDynamoDBBuilderExtensions
    {
        /// <summary>
        /// Add DynamoDB Operational Store.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsBuilder">DynamoDB Operational Store Options builder</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore(this IIdentityServerBuilder builder, Action<DynamoDBOptions> optionsBuilder = null)
        {
            var options = new DynamoDBOptions();
            optionsBuilder?.Invoke(options);

            if (options.DynamoDBContextConfig is null)
                options.DynamoDBContextConfig = new Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig();

            builder.Services.AddSingleton<DynamoDBOptions>(options);
            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            return builder;
        }
    }
}