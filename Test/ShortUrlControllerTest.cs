using Domains.Domains;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repository;
using Repository.Repositories;
using Services;
using Services.DataServices;
using Web.Controllers;

namespace Test {
    public class ShortUrlControllerTest {
        private async Task<IEnumerable<ShortUrl>> GetTestShortUrls() {
            var urls = new List<string>() {
                "https://www.google.com/",
                "https://github.com/",
                "https://www.youtube.com/",
                "https://mail.google.com/mail/u/1/?dispatcher_command=master_lookup#inbox",
                "https://www.kinopoisk.ru/"
            };
            return urls
                .Select(url => new ShortUrl() {
                    Id = 1,
                    Url = url,
                    Hash = HashGenerator.GenerateHash(url),
                    CreatingDate = DateTime.Now,
                    TransitionsCount = 0})
                .ToList();
        }

        private async Task<ShortUrl> GetTestShortUrl() {
            string url = "https://www.google.com/";
            return new() {
                Id = 1,
                Url = url,
                Hash = HashGenerator.GenerateHash(url),
                CreatingDate = DateTime.Now,
                TransitionsCount = 0,
            };
        }

        [Fact]
        public async Task Index_Test() {
            // Arrange
            var mock = new Mock<IRepository<ShortUrl>>();
            mock.Setup(repo => repo.GetAsync()).Returns(GetTestShortUrls());
            var dataService = new ShortUrlDataService(mock.Object);
            var controller = new ShortUrlController(dataService);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ShortUrl>>(result.Model);
            Assert.Equal(5, ((List<ShortUrl>)result.Model).Count);
        }

        [Fact]
        public async Task RedirectToUrl_WhenHashExist() {
            // Arrange
            var shortUrl = await GetTestShortUrl();
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase5")
            .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(shortUrl);
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);
                var controller = new ShortUrlController(dataService);
                var result = await controller.RedirectToUrl(shortUrl.Hash) as RedirectResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, context.ShortUrls.Find(1).TransitionsCount);
            }
        }

        [Fact]
        public async Task RedirectToUrl_WhenHashDoesNotExist() {
            // Arrange
            var shortUrl = await GetTestShortUrl();
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase6")
            .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(shortUrl);
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);
                var controller = new ShortUrlController(dataService);
                var result = await controller.RedirectToUrl("test") as RedirectToActionResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Error", result.ActionName);
                Assert.Equal("The database does not contain this URL", result.RouteValues["message"]);
            }
        }
    }
}