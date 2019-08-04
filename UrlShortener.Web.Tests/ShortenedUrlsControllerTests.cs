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
using Xunit;

namespace UrlShortener.Web.Tests
{
    public class ShortenedUrlsControllerTests
    {

        public class GetShortenedUrl
        {

            [Fact]
            public async Task WhenInvalidIdSupplied_ReturnBadRequest()
            {
                // Setup
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("invalidId")).Returns(false);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync((ShortenedUrl)null);

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
            [Fact]
            public async Task WhenInvalidIdSupplied_ReturnBadRequest()
            {
                // Setup
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("invalidId")).Returns(false);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync(new ShortenedUrl()
                {
                    Id = "validId",
                    ShortUrl = "https://shorturl.test.com/validId",
                    LongUrl = "http://mylongurl.com/abc/?id=zyx"
                });

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync((ShortenedUrl)null);
                mockStore.Setup(_ => _.AddAsync(It.Is<ShortenedUrl>(x => x.Id == "validId"), null, CancellationToken.None))
                    .ReturnsAsync((Cosmonaut.Response.CosmosResponse<ShortenedUrl>)null);

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
            [Fact]
            public async Task WhenExistingLongUrlFound_ReturnOk()
            {
                // Setup
                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);

                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.Query(null)).Returns(new List<ShortenedUrl>()
                {
                    new ShortenedUrl()
                    {
                        Id = "validId",
                        ShortUrl = "https://shorturl.test.com/validId",
                        LongUrl = "http://mylongurl.com/abc/?id=zyx"
                    }}.AsQueryable());

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
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
                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);

                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.Query(null)).Returns(new List<ShortenedUrl>().AsQueryable());
                mockStore.Setup(_ => _.AddAsync(It.IsAny<ShortenedUrl>(), null, CancellationToken.None))
                    .ReturnsAsync((Cosmonaut.Response.CosmosResponse<ShortenedUrl>)null);

                var config = new SiteConfig() { ShortenUrlHostName = "https://shorturl.test.com" };

                // Action
                var sut = new ShortenedUrlsController(mockStore.Object, config, mockValidator.Object);
                var actionResult = await sut.PostShortenedUrl(new Models.NewShortenedUrl() { LongUrl = "http://mylongurl.com/abc/?id=zyx" });

                // Assert
                Assert.NotNull(actionResult);
                Assert.NotNull(actionResult.Result);
                var createResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var returnValue = Assert.IsType<ShortenedUrl>(createResult.Value);
                Assert.True(returnValue.Id.Length == 8);
                Assert.Equal("https://shorturl.test.com/" + returnValue.Id, returnValue.ShortUrl);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", returnValue.LongUrl);
            }
        }


    }
}
