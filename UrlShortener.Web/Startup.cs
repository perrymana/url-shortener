using Microsoft.AspNetCore.Builder;
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
using UrlShortener.Common.Config;
using UrlShortener.Common.Data;
using Microsoft.Extensions.Logging;
using UrlShortener.Common.Validation;
using UrlShortener.Web.Services;

namespace UrlShortener.Web
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureServicesDI(services);
            ConfigureServicesMvc(services);
            ConfigureServicesDatabase(services);
            ConfigureServicesSwagger(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ConfigureMvc(app, env);
        }
    }
}
