using Domains.Domains;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace Repository.Repositories {
    public class ShortUrlRepository : IRepository<ShortUrl> {
        private readonly UrlShortnerContext _context;

        public ShortUrlRepository(UrlShortnerContext context) {
            _context = context;
        }

        public async Task<ShortUrl> AddAsync(ShortUrl entity) {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id) {
            var shortUrl = await _context.ShortUrls.FindAsync(id);
            if (shortUrl != null) {
                _context.ShortUrls.Remove(shortUrl);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ShortUrl>> GetAsync() {
            return await _context.ShortUrls.ToListAsync();
        }

        public async Task<ShortUrl> GetAsync(int id) {
            var shortUrl = await _context.ShortUrls.FindAsync(id);
            return shortUrl ?? throw new SqlNullValueException("The database does not contain this URL");
        }

        public async Task UpdateAsync(ShortUrl entity) {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
