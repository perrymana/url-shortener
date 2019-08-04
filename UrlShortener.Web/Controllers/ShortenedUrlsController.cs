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
using UrlShortener.Web.Services;

namespace UrlShortener.Web.Controllers
{
    /// <summary>
    /// Primary API Controller for creating and retrieving shortened urls.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ShortenedUrlsController : ControllerBase
    {
        private readonly ICosmosStore<ShortenedUrl> urlStore;
        private readonly SiteConfig configuration;
        private readonly IAliasValidator validator;
        private readonly IShortUrlGenerator shortUrlGenerator;

        public ShortenedUrlsController(ICosmosStore<ShortenedUrl> urlStore, SiteConfig configuration, IAliasValidator validator, IShortUrlGenerator shortUrlGenerator)
        {
            this.urlStore = urlStore;
            this.configuration = configuration;
            this.validator = validator;
            this.shortUrlGenerator = shortUrlGenerator;
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
        /// Creates new shortened url with a supplied id
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
        /// Creates new shortened url with a random id
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

            var id = shortUrlGenerator.GenerateNewId();
            // TODO - Geneate a different id if it already exists?
            var shortenedUrl = await CreateNew(id, newShortenedUrl);

            return CreatedAtAction("GetShortenedUrl", new { id = shortenedUrl.Id }, shortenedUrl);
        }

        /// <summary>
        /// Creates new entry in data store
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newShortenedUrl"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Combines short url id with the root hostname.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string BuildShortUrl(string id)
        {
            var hostName = this.configuration.ShortenUrlHostName;
            return hostName + "/" + id;
        }
        
    }
}