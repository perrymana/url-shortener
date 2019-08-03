using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ShortenedUrlsController : ControllerBase
    {
        private readonly UrlShortenerWebContext _context;

        public ShortenedUrlsController(UrlShortenerWebContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retreives the details of an existing shortened url
        /// </summary>
        /// <param name="alias">Url alias that gets added to the hostname</param>
        /// <returns></returns>
        [HttpGet("{alias}")]
        public async Task<ActionResult<ShortenedUrl>> GetShortenedUrl([FromRoute] string alias)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shortenedUrl = await _context.ShortenedUrl.FindAsync(alias);

            if (shortenedUrl == null)
            {
                return NotFound();
            }

            return Ok(shortenedUrl);
        }

        /// <summary>
        /// Creates new shortened url with supplied alias
        /// </summary>
        /// <param name="alias">Url alias that gets added to the hostname</param>
        /// <param name="newShortenedUrl">Object containing the url to shorten</param>
        /// <returns></returns>
        [HttpPut("{alias}")]
        public async Task<ActionResult<ShortenedUrl>> PutShortenedUrl([FromRoute] string alias, [FromBody] NewShortenedUrl newShortenedUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ShortenedUrl shortenedUrl = await CreateNew(alias, newShortenedUrl);
                return CreatedAtAction("GetShortenedUrl", new { alias = shortenedUrl.Alias }, shortenedUrl);
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
        /// Creates new shortened url with random alias
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

            var alias = GenerateNewAlias();
            // TODO - Geneate a different alias if it already exists?

            ShortenedUrl shortenedUrl = await CreateNew(alias, newShortenedUrl);


            return CreatedAtAction("GetShortenedUrl", new { alias = shortenedUrl.Alias }, shortenedUrl);
        }

        private async Task<ShortenedUrl> CreateNew(string alias, NewShortenedUrl newShortenedUrl)
        {
            var shortenedUrl = new ShortenedUrl()
            {
                Alias = alias,
                LongUrl = newShortenedUrl.LongUrl,
                ShortUrl = BuildShortUrl(alias)
            };

            _context.ShortenedUrl.Add(shortenedUrl);
            await _context.SaveChangesAsync();
            return shortenedUrl;
        }

        private string BuildShortUrl(string alias)
        {
            var hostName = "https://localhost:5001"; // TODO
            return hostName + "/" + alias;
        }
        
        /// <summary>
        /// Generates a new alias for a short url. Utilises a hashing algorithm to generate random "enough" strings.
        /// </summary>
        /// <returns></returns>
        private string GenerateNewAlias()
        {
            // TEMP - HashLongUrl
            //byte[] buffer = Encoding.UTF8.GetBytes(System.Guid.NewGuid().ToByteArray);
            byte[] buffer = System.Guid.NewGuid().ToByteArray();

            var sha1 = System.Security.Cryptography.SHA1.Create();

            var hash = sha1.ComputeHash(buffer);
            var hashString = Convert.ToBase64String(hash);

            // Grab the first 8 characters from hash as a our alias.
            return hashString.Substring(0, 8);
        }

        //private bool ShortenedUrlExists(string id)
        //{
        //    return _context.ShortenedUrl.Any(e => e.Alias == id);
        //}
    }
}