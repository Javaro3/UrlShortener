using Domains.Domains;
using Microsoft.EntityFrameworkCore;

namespace Repository {
    public class UrlShortnerContext : DbContext {
        public DbSet<ShortUrl> ShortUrls { get; set; }

        public UrlShortnerContext(DbContextOptions<UrlShortnerContext> options)
        : base(options) {
        }
    }
}