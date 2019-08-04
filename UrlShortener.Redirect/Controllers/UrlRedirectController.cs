using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Common.Data;
using UrlShortener.Common.Validation;

namespace UrlShortener.Web.Controllers
{
    public class UrlRedirectController : Controller
    {
        private readonly ICosmosStore<ShortenedUrl> urlStore;
        private readonly IAliasValidator validator;

        public UrlRedirectController(ICosmosStore<ShortenedUrl> urlStore, IAliasValidator validator)
        {
            this.urlStore = urlStore;
            this.validator = validator;
        }

        /// <summary>
        /// Redirect to the long url associated with the supplied id.
        /// </summary>
        /// <param name="id">url alias</param>
        /// <returns></returns>
        public async Task<IActionResult> RedirectToLongUrl(string id)
        {
            // Is the id provided a valid alias?
            if (!validator.IsValid(id))
            {
                return BadRequest();
            }

            // Search data store for the long version
            var shortenedUrl = await urlStore.FindAsync(id);
            if (shortenedUrl == null)
            {
                return NotFound();
            }

            // Redirect the browser
            return Redirect(shortenedUrl.LongUrl);
        }
    }
}