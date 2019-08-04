using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Common.Config;
using UrlShortener.Common.Data;

namespace UrlShortener.Redirect
{
    public partial class Startup
    {

        public void ConfigureServicesDatabase(IServiceCollection services)
        {
            var cosmosConfig = Configuration.GetSection("Cosmos")?.Get<CosmosConfig>();

            this.Logger.LogInformation("Connecting to Cosmos DB {DatabaseName} at {CosmosUri}", cosmosConfig?.DatabaseName, cosmosConfig?.CosmosUri);
            if (cosmosConfig == null || string.IsNullOrEmpty(cosmosConfig.DatabaseName) || string.IsNullOrEmpty(cosmosConfig.CosmosUri) || string.IsNullOrEmpty(cosmosConfig.AuthKey))
            {
                throw new ApplicationException("Cosmos Configuration Invalid");
            }

            var cosmosSettings = new CosmosStoreSettings(cosmosConfig.DatabaseName, cosmosConfig.CosmosUri, cosmosConfig.AuthKey, settings =>
            {
                //settings.ConnectionPolicy = connectionPolicy;
                //settings.DefaultCollectionThroughput = 5000;
                //settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1),
                //    new RangeIndex(DataType.String, -1));
            });
            services.AddCosmosStore<ShortenedUrl>(cosmosSettings);

        }

    }
}
