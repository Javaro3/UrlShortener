using Domains.Domains;
using Domains.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Repository;
using Repository.Repositories;
using Services;
using Services.DataServices;
using System.Data.SqlTypes;

namespace Test {
    public class ShortUrlDataServiceTest {
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
        public async Task GetAll() {
            // Arrange
            var mock = new Mock<IRepository<ShortUrl>>();
            mock.Setup(e => e.GetAsync()).Returns(GetTestShortUrls());
            var dataService = new ShortUrlDataService(mock.Object);

            // Act
            var shortUrls = await dataService.GetAsync();

            // Assert
            Assert.NotNull(shortUrls);
            Assert.IsAssignableFrom<IEnumerable<ShortUrl>>(shortUrls);
            Assert.Equal(5, shortUrls.Count());
        }

        [Fact]
        public async Task GetById_WhenIdExist() {
            // Arrange
            var mock = new Mock<IRepository<ShortUrl>>();
            mock.Setup(e => e.GetAsync(1)).Returns(GetTestShortUrl());
            var dataService = new ShortUrlDataService(mock.Object);

            // Act
            var shortUrl = await dataService.GetAsync(1);

            // Assert
            Assert.NotNull(shortUrl);
            Assert.IsAssignableFrom<ShortUrl>(shortUrl);
            Assert.Equal(1, shortUrl.Id);
        }

        [Fact]
        public async Task GetById_WhenIdDoesNotExist() {
            // Arrange
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase1")
                .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(await GetTestShortUrl());
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);
                // Act and Assert
                await Assert.ThrowsAsync<SqlNullValueException>(async () => await dataService.GetAsync(2));
            }
        }

        [Fact]
        public async Task Delete_WhenObjectExist() {
            // Arrange
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
            .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(await GetTestShortUrl());
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);

                // Act and Assert
                await dataService.DeleteAsync(1);
                
                // Assert
                Assert.Equal(0, context.ShortUrls.Count());
            }
        }

        [Fact]
        public async Task Delete_WhenObjectDoesNotExist() {
            // Arrange
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase3")
            .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(await GetTestShortUrl());
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);

                // Act and Assert
                await dataService.DeleteAsync(2);

                // Assert
                Assert.Equal(1, context.ShortUrls.Count());
            }
        }

        [Fact]
        public async Task Add_WhenObjectExistInDatabase() {
            // Arrange
            var addModel = new AddShortUrlViewModel() { Url = "https://www.google.com/" };
            var model = new ShortUrl() {
                CreatingDate = DateTime.Now,
                TransitionsCount = 0,
                Url = addModel.Url,
                Hash = HashGenerator.GenerateHash(addModel.Url)
            };
            var mock = new Mock<IRepository<ShortUrl>>();
            var dataService = new ShortUrlDataService(mock.Object);
            mock.Setup(e => e.GetAsync()).Returns(GetTestShortUrls());

            // Act and Assert
            await Assert.ThrowsAsync<SqlAlreadyFilledException>(async () => await dataService.AddAsync(addModel));
        }

        [Fact]
        public async Task GetByHash_WhenHashExistInDatabase() {
            // Arrange
            var addModel = new AddShortUrlViewModel() { Url = "https://www.google.com/" };
            var model = new ShortUrl() {
                CreatingDate = DateTime.Now,
                TransitionsCount = 0,
                Url = addModel.Url,
                Hash = HashGenerator.GenerateHash(addModel.Url)
            };
            var mock = new Mock<IRepository<ShortUrl>>();
            var dataService = new ShortUrlDataService(mock.Object);
            mock.Setup(e => e.GetAsync()).Returns(GetTestShortUrls());

            // Act
            var result = await dataService.GetByHashAsync(model.Hash);

            // Assert
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByHash_WhenHashDoesNotExistInDatabase() {
            // Arrange
            var addModel = new AddShortUrlViewModel() { Url = "a" };
            var model = new ShortUrl() {
                CreatingDate = DateTime.Now,
                TransitionsCount = 0,
                Url = addModel.Url,
                Hash = HashGenerator.GenerateHash(addModel.Url)
            };
            var mock = new Mock<IRepository<ShortUrl>>();
            var dataService = new ShortUrlDataService(mock.Object);
            mock.Setup(e => e.GetAsync()).Returns(GetTestShortUrls());

            // Act and Assert
            await Assert.ThrowsAsync<SqlNullValueException>(async () => await dataService.GetByHashAsync(model.Hash));
        }

        [Fact]
        public async Task IncrementTransitionsCountTest() {
            // Arrange
            var options = new DbContextOptionsBuilder<UrlShortnerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase4")
            .Options;

            using (var context = new UrlShortnerContext(options)) {
                await context.ShortUrls.AddAsync(await GetTestShortUrl());
                await context.SaveChangesAsync();
            }

            using (var context = new UrlShortnerContext(options)) {
                var repo = new ShortUrlRepository(context);
                var dataService = new ShortUrlDataService(repo);

                await dataService.IncrementTransitionsCountAsync(await GetTestShortUrl());

                // Assert
                Assert.Equal(1, context.ShortUrls.Find(1).TransitionsCount);
            }
        }
    }
}
