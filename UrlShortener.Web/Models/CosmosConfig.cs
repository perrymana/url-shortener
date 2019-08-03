using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Web.Models
{
    public class CosmosConfig
    {
        public string DatabaseName { get; set; }
        public string CosmosUri { get; set; }
        public string AuthKey { get; set; }
    }
}
