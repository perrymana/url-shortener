using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Web.Models
{
    public class PutShortenedUrlRequest
    {
        /// <summary>
        /// Url id that gets added to the hostname
        /// </summary>
        [Required]
        [FromRoute]
        public string id { get; set; }


        /// <summary>
        /// The long url to shorten
        /// </summary>
        [Required]
        [Url]
        public string LongUrl { get; set; }
    }
}
