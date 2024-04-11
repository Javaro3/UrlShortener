using Domains.Domains;
using Domains.ViewModels;
using Repository.Repositories;
using System.Data.SqlTypes;

namespace Services.DataServices {
    public class ShortUrlDataService {
        private readonly ShortUrlRepository _repository;

        public ShortUrlDataService(ShortUrlRepository repository) {
            _repository = repository;
        }

        public async Task<IEnumerable<ShortUrl>> GetAsync() {
            return await _repository.GetAsync();
        }

        public async Task<ShortUrl> GetAsync(int id) {
            return await _repository.GetAsync(id);
        }

        public async Task AddAsync(AddShortUrlViewModel model) {
            var shortUrl = new ShortUrl() {
                CreatingDate = DateTime.Now,
                TransitionsCount = 0,
                Url = model.Url,
                Hash = HashGenerator.GenerateHash(model.Url)
            };

            var shortUrls = await GetAsync();
            if (shortUrls.FirstOrDefault(e => e.Url == model.Url) != null) {
                throw new SqlAlreadyFilledException("URL is already exist");
            }

            await _repository.AddAsync(shortUrl);
        }

        public async Task UpdateAsync(UpdateShortUrlViewModel model) {
            var shortUrl = await GetAsync(model.Id);

            var shortUrls = await GetAsync();
            if (shortUrls.FirstOrDefault(e => e.Url == model.Url) != null) {
                throw new SqlAlreadyFilledException("URL is already exist");
            }

            shortUrl.Url = model.Url;
            shortUrl.Hash = HashGenerator.GenerateHash(model.Url);
            shortUrl.TransitionsCount = 0;
            await _repository.UpdateAsync(shortUrl);
        }

        public async Task DeleteAsync(int id) {
            await _repository.DeleteAsync(id);
        }

        public async Task<ShortUrl> GetByHashAsync(string hash) {
            var shortUrls = await GetAsync();
            var shortUrl = shortUrls.FirstOrDefault(e => e.Hash == hash);
            return shortUrl ?? throw new SqlNullValueException("The database does not contain this URL");
        }

        public async Task<int> IncrementTransitionsCount(ShortUrl shortUrl) {
            shortUrl.TransitionsCount++;
            await _repository.UpdateAsync(shortUrl);
            return shortUrl.TransitionsCount;
        }
    }
}
