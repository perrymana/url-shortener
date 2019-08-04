using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Web.Services
{
    public interface IShortUrlGenerator
    {

        /// <summary>
        /// Generates a new id for a short url
        /// </summary>
        /// <returns></returns>
        string GenerateNewId();
    }
}
