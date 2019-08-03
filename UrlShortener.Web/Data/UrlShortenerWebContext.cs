using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Web.Models
{
    public class UrlShortenerWebContext : DbContext
    {
        public UrlShortenerWebContext (DbContextOptions<UrlShortenerWebContext> options)
            : base(options)
        {
        }

        public DbSet<UrlShortener.Web.Models.ShortenedUrl> ShortenedUrl { get; set; }
    }
}
