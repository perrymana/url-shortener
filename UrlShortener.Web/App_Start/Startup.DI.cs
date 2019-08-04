using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Common.Config;
using UrlShortener.Common.Validation;
using UrlShortener.Web.Services;

namespace UrlShortener.Web
{
    public partial class Startup
    {
        public void ConfigureServicesDI(IServiceCollection services)
        {
            var hostName = this.Configuration.GetValue<string>("ShortenUrlHostName");
            this.Logger.LogInformation("Config: {ShortenUrlHostName}", hostName);

            services.AddSingleton(new SiteConfig() { ShortenUrlHostName = hostName });
            // Add custom validator
            services.AddSingleton<IAliasValidator, AliasValidator>();
            services.AddSingleton<IShortUrlGenerator, HashUrlGenerator>();
        }
    }
}
