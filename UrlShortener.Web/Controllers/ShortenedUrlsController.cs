using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cosmonaut;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Common.Data;
using UrlShortener.Common.Validation;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ShortenedUrlsController : ControllerBase
    {
        private readonly ICosmosStore<ShortenedUrl> urlStore;
        private readonly IConfiguration configuration;

        public ShortenedUrlsController(ICosmosStore<ShortenedUrl> urlStore, IConfiguration configuration)
        {
            this.urlStore = urlStore;
            this.configuration = configuration;
        }

        /// <summary>
        /// Retreives the details of an existing shortened url
        /// </summary>
        /// <param name="id">Url id that gets added to the hostname</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ShortenedUrl>> GetShortenedUrl([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!AliasValidation.IsValid(id))
            {
                return BadRequest("Invalid Id Format");
            }

            var shortenedUrl = await urlStore.FindAsync(id);

            if (shortenedUrl == null)
            {
                return NotFound();
            }

            return Ok(shortenedUrl);
        }

        /// <summary>
        /// Creates new shortened url with supplied id
        /// </summary>
        /// <param name="id">Url id that gets added to the hostname</param>
        /// <param name="newShortenedUrl">Object containing the url to shorten</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ShortenedUrl>> PutShortenedUrl([FromRoute] string id, [FromBody] NewShortenedUrl newShortenedUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!AliasValidation.IsValid(id))
            {
                return BadRequest("Invalid Id Format");
            }

            try
            {
                ShortenedUrl shortenedUrl = await CreateNew(id, newShortenedUrl);
                return CreatedAtAction("GetShortenedUrl", new { id = shortenedUrl.Id }, shortenedUrl);
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException?.Message.Contains("Violation of PRIMARY KEY constraint", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    return Conflict("Already exists");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates new shortened url with random id
        /// </summary>
        /// <param name="newShortenedUrl">Object containing the url to shorten</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ShortenedUrl>> PostShortenedUrl([FromBody] NewShortenedUrl newShortenedUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = GenerateNewId();
            // TODO - Geneate a different id if it already exists?

            ShortenedUrl shortenedUrl = await CreateNew(id, newShortenedUrl);


            return CreatedAtAction("GetShortenedUrl", new { id = shortenedUrl.Id }, shortenedUrl);
        }

        private async Task<ShortenedUrl> CreateNew(string id, NewShortenedUrl newShortenedUrl)
        {
            var shortenedUrl = new ShortenedUrl()
            {
                Id = id,
                LongUrl = newShortenedUrl.LongUrl,
                ShortUrl = BuildShortUrl(id)
            };

            await urlStore.AddAsync(shortenedUrl);
            return shortenedUrl;
        }

        private string BuildShortUrl(string id)
        {
            var hostName = this.configuration.GetValue<string>("ShortenUrlHostName");  //  "https://localhost:5001"; // TODO
            return hostName + "/" + id;
        }
        
        /// <summary>
        /// Generates a new id for a short url. Utilises a hashing algorithm to generate random "enough" strings.
        /// </summary>
        /// <returns></returns>
        private string GenerateNewId()
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

        //private bool ShortenedUrlExists(string id)
        //{
        //    return _context.ShortenedUrl.Any(e => e.Id == id);
        //}
    }
}