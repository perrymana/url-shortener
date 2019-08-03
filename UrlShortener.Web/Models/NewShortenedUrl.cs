using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Web.Models
{
    public class NewShortenedUrl
    {
        /// <summary>
        /// The long url to shorten
        /// </summary>
        [Required]
        [Url]
        public string LongUrl { get; set; }
    }
}
