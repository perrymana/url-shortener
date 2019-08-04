using Cosmonaut;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UrlShortener.Common.Config;
using UrlShortener.Common.Data;
using UrlShortener.Common.Validation;
using UrlShortener.Web.Controllers;
using UrlShortener.Web.Services;
using Xunit;

namespace UrlShortener.Web.Tests
{
    public class ShortenedUrlsControllerTests
    {

        public class GetShortenedUrl
        {
            private Mock<ICosmosStore<ShortenedUrl>> mockStore;
            private Mock<IAliasValidator> mockValidator;
            private Mock<IShortUrlGenerator> mockGenerator;
            private SiteConfig config;
            private ShortenedUrlsController sut;

            public GetShortenedUrl()
            {
                mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockGenerator = new Mock<IShortUrlGenerator>(MockBehavior.Strict);
                config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object, mockGenerator.Object);
            }

            [Fact]
            public async Task WhenInvalidIdSupplied_ReturnBadRequest()
            {
                // Setup
                mockValidator.Setup(_ => _.IsValid("invalidId")).Returns(false);

                // Action
                var actionResult = await sut.GetShortenedUrl("invalidId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            }

            [Fact]
            public async Task WhenIdNotFound_ReturnNotFound()
            {
                // Setup
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync((ShortenedUrl)null);

                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var actionResult = await sut.GetShortenedUrl("validId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                Assert.IsType<NotFoundResult>(actionResult.Result);
            }

            [Fact]
            public async Task WhenIdFound_ReturnShortendUrl()
            {
                // Setup
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var actionResult = await sut.GetShortenedUrl("validId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(okResult.Value);
                Assert.Equal("validId", returnValue.Id);
                Assert.Equal("https://shorturl.test.com/validId", returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }

        }

        public class PutShortenedUrl
        {
            private Mock<ICosmosStore<ShortenedUrl>> mockStore;
            private Mock<IAliasValidator> mockValidator;
            private Mock<IShortUrlGenerator> mockGenerator;
            private SiteConfig config;
            private ShortenedUrlsController sut;

            public PutShortenedUrl()
            {
                mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockGenerator = new Mock<IShortUrlGenerator>(MockBehavior.Strict);
                config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object, mockGenerator.Object);
            }

            [Fact]
            public async Task WhenInvalidIdSupplied_ReturnBadRequest()
            {
                // Setup
                mockValidator.Setup(_ => _.IsValid("invalidId")).Returns(false);

                // Action
                var actionResult = await sut.PutShortenedUrl("invalidId", new Models.NewShortenedUrl() { LongUrl = "https://my.long.url/?abc" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            }

            [Fact]
            public async Task WhenExistingIdWithSameLongUrlFound_ReturnOk()
            {
                // Setup
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var actionResult = await sut.PutShortenedUrl("validId", new Models.NewShortenedUrl() { LongUrl = "http://mylongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(okResult.Value);
                Assert.Equal("validId", returnValue.Id);
                Assert.Equal("https://shorturl.test.com/validId", returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }

            [Fact]
            public async Task WhenExistingIdWithDifferentLongUrlFound_ReturnConflict()
            {
                // Setup
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var actionResult = await sut.PutShortenedUrl("validId", new Models.NewShortenedUrl() { LongUrl = "http://mydifferentlongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                Assert.IsType<ConflictObjectResult>(actionResult.Result);
            }

            [Fact]
            public async Task WhenNewIdSupplied_Creates_And_ReturnCreated()
            {
                // Setup
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync((ShortenedUrl)null);
                mockStore.Setup(_ => _.AddAsync(It.Is<ShortenedUrl>(x => x.Id == "validId"), null, CancellationToken.None))
                    .ReturnsAsync((Cosmonaut.Response.CosmosResponse<ShortenedUrl>)null);

                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var actionResult = await sut.PutShortenedUrl("validId", new Models.NewShortenedUrl() { LongUrl = "http://mylongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(createdResult.Value);
                Assert.Equal("validId", returnValue.Id);
                Assert.Equal("https://shorturl.test.com/validId", returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }
        }

        public class PostShortenedUrl
        {
            private Mock<ICosmosStore<ShortenedUrl>> mockStore;
            private Mock<IAliasValidator> mockValidator;
            private Mock<IShortUrlGenerator> mockGenerator;
            private SiteConfig config;
            private ShortenedUrlsController sut;

            public PostShortenedUrl()
            {
                mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockGenerator = new Mock<IShortUrlGenerator>(MockBehavior.Strict);
                config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object, mockGenerator.Object);
            }

            [Fact]
            public async Task WhenExistingLongUrlFound_ReturnOk()
            {
                // Setup
                mockStore.Setup(_ => _.Query(null)).Returns(new List<ShortenedUrl>()
                {
                    new ShortenedUrl()
                    {
                        Id = "validId",
                        ShortUrl = "https://shorturl.test.com/validId",
                        LongUrl = "http://mylongurl.com/abc/?id=zyx"
                    }}.AsQueryable());

                // Action
                var actionResult = await sut.PostShortenedUrl(new Models.NewShortenedUrl() { LongUrl = "http://mylongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(okResult.Value);
                Assert.Equal("validId", returnValue.Id);
                Assert.Equal("https://shorturl.test.com/validId", returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }

            [Fact]
            public async Task WhenExistingLongUrlNotFound_Creates_And_ReturnCreated()
            {
                // Setup
                mockStore.Setup(_ => _.Query(null)).Returns(new List<ShortenedUrl>().AsQueryable());
                mockStore.Setup(_ => _.AddAsync(It.IsAny<ShortenedUrl>(), null, CancellationToken.None))
                    .ReturnsAsync((Cosmonaut.Response.CosmosResponse<ShortenedUrl>)null);

                mockGenerator.Setup(_ => _.GenerateNewId()).Returns("myNewId");

                // Action
                var actionResult = await sut.PostShortenedUrl(new Models.NewShortenedUrl() { LongUrl = "http://mylongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var createResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(createResult.Value);
                Assert.Equal("myNewId", returnValue.Id);
                Assert.Equal("https://shorturl.test.com/myNewId", returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }
        }


    }
}
