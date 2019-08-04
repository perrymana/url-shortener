using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Web.Services
{
    public class HashUrlGenerator : IShortUrlGenerator
    {
        /// <summary>
        /// Generates a new id for a short url. Utilises a hashing algorithm to generate random "enough" strings.
        /// </summary>
        /// <returns></returns>
        public string GenerateNewId()
        {
            // TEMP - HashLongUrl
            //byte[] buffer = Encoding.UTF8.GetBytes(System.Guid.NewGuid().ToByteArray);
            byte[] buffer = System.Guid.NewGuid().ToByteArray();

            var sha1 = System.Security.Cryptography.SHA1.Create();

            var hash = sha1.ComputeHash(buffer);
            var hashString = Convert.ToBase64String(hash);

            // Replace "/" with -
            hashString = hashString.Replace("/", "-");

            // Grab the first 8 characters from hash as a our id.
            return hashString.Substring(0, 8);
        }
    }
}
