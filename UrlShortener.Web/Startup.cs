﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Models;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Cosmonaut;

namespace UrlShortener.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            var cosmosConfig = Configuration.GetSection("Cosmos").Get<CosmosConfig>();
            var cosmosSettings = new CosmosStoreSettings(cosmosConfig.DatabaseName, cosmosConfig.CosmosUri, cosmosConfig.AuthKey, settings =>
            {
                //settings.ConnectionPolicy = connectionPolicy;
                //settings.DefaultCollectionThroughput = 5000;
                //settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1),
                //    new RangeIndex(DataType.String, -1));
            });
            services.AddCosmosStore<ShortenedUrl>(cosmosSettings);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Smaller Urls API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
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
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smaller Urls API V1");
            });

            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //    name: "default",
                //    template: "{controller}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "redirecttolongurl",
                    template: "{id:regex(" + Controllers.ShortenedUrlsController.Base64IshRegexStr + ")}", // HACK
                    defaults: new
                    {
                        controller = "UrlRedirect",
                        action = "RedirectToLongUrl"
                    }
                );

            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
