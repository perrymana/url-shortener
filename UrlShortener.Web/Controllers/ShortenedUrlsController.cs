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
using UrlShortener.Common.Config;
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
        private readonly SiteConfig configuration;
        private readonly IAliasValidator validator;

        public ShortenedUrlsController(ICosmosStore<ShortenedUrl> urlStore, SiteConfig configuration, IAliasValidator validator)
        {
            this.urlStore = urlStore;
            this.configuration = configuration;
            this.validator = validator;
        }

        /// <summary>
        /// Retreives the details of an existing shortened url
        /// </summary>
        /// <param name="id">Url id that gets added to the hostname</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortenedUrl>> GetShortenedUrl([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!validator.IsValid(id))
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortenedUrl>> PutShortenedUrl([FromRoute] string id, [FromBody] NewShortenedUrl newShortenedUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!validator.IsValid(id))
            {
                return BadRequest("Invalid Id Format");
            }

            // Check to see if a shortened url with this id already exists.
            // (Yeah, there is a minor race condition here)
            var existingShortenedUrl = await urlStore.FindAsync(id);
            if (existingShortenedUrl != null)
            {
                // It does, but does the long url match?
                if (existingShortenedUrl.LongUrl == newShortenedUrl.LongUrl)
                {
                    return Ok(existingShortenedUrl);
                }
                else
                {
                    return Conflict("Already exists");
                }
            }

            ShortenedUrl shortenedUrl = await CreateNew(id, newShortenedUrl);
            return CreatedAtAction("GetShortenedUrl", new { id = shortenedUrl.Id }, shortenedUrl);
        }

        /// <summary>
        /// Creates new shortened url with random id
        /// </summary>
        /// <param name="newShortenedUrl">Object containing the url to shorten</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortenedUrl>> PostShortenedUrl([FromBody] NewShortenedUrl newShortenedUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // See if we have already registered this particular url. If we have return it without generating a new one.
            // (Yeah, there is a minor race condition here)

            // Unfortunately I can't get the async versions to work...
            //var shortenedUrl = await urlStore.Query().FirstOrDefaultAsync(x => x.LongUrl == newShortenedUrl.LongUrl);
            var existingShortenedUrl = urlStore.Query().Where(x => x.LongUrl == newShortenedUrl.LongUrl).ToList().FirstOrDefault();
            if (existingShortenedUrl != null)
            {
                return Ok(existingShortenedUrl);
            }

            var id = GenerateNewId();
            // TODO - Geneate a different id if it already exists?
            var shortenedUrl = await CreateNew(id, newShortenedUrl);

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
            var hostName = this.configuration.ShortenUrlHostName;
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

    }
}