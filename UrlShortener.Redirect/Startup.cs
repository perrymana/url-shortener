using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UrlShortener.Common.Config;
using UrlShortener.Common.Data;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Common.Validation;

namespace UrlShortener.Redirect
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

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

            // Add custom validator
            services.AddSingleton<IAliasValidator, AliasValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "redirecttolongurl",
                    template: "{id}", 
                    defaults: new
                    {
                        controller = "UrlRedirect",
                        action = "RedirectToLongUrl"
                    }
                );

            });
        }
    }
}
