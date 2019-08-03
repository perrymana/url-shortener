using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    public class UrlRedirectController : Controller
    {
        private readonly UrlShortenerWebContext _context;

        public UrlRedirectController(UrlShortenerWebContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> RedirectToLongUrl(string alias)
        {
            var shortenedUrl = await _context.ShortenedUrl.FindAsync(alias);

            if (shortenedUrl == null)
            {
                return NotFound();
            }

            return Redirect(shortenedUrl.LongUrl);
        }
    }
}