using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Common.Config;
using UrlShortener.Common.Validation;

namespace UrlShortener.Redirect
{
    public partial class Startup
    {
        public void ConfigureServicesDI(IServiceCollection services)
        {

            // Add custom validator
            services.AddSingleton<IAliasValidator, AliasValidator>();

        }
    }
}
