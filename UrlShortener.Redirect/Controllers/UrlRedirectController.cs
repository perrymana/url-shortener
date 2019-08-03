﻿using System;
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
        private readonly ICosmosStore<ShortenedUrl> _urlStore;

        public UrlRedirectController(ICosmosStore<ShortenedUrl> urlStore)
        {
            _urlStore = urlStore;
        }


        public async Task<IActionResult> RedirectToLongUrl(string id)
        {
            var shortenedUrl = await _urlStore.FindAsync(id);

            if (shortenedUrl == null)
            {
                return NotFound();
            }

            if (!AliasValidation.IsValid(id))
            {
                return NotFound();
            }

            return Redirect(shortenedUrl.LongUrl);
        }
    }
}