using Cosmonaut;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UrlShortener.Common.Data;
using UrlShortener.Common.Validation;
using UrlShortener.Web.Controllers;
using Xunit;

namespace UrlShortener.Redirect.Tests
{
    public class UrlRedirectControllerTests
    {
        public class RedirectToLongUrl
        {

            [Fact]
            public async Task WhenInvalidIdSupplied_ReturnBadRequest()
            {
                // Setup
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("invalidId")).Returns(false);

                // Action
                var sut = new UrlRedirectController(mockStore.Object, mockValidator.Object);
                var actionResult = await sut.RedirectToLongUrl("invalidId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.IsType<BadRequestResult>(actionResult);
            }

            [Fact]
            public async Task WhenIdNotFound_ReturnNotFound()
            {
                // Setup
                var mockStore = new Mock<ICosmosStore<ShortenedUrl>>(MockBehavior.Strict);
                mockStore.Setup(_ => _.FindAsync("validId", null, CancellationToken.None)).ReturnsAsync((ShortenedUrl)null);

                var mockValidator = new Mock<IAliasValidator>(MockBehavior.Strict);
                mockValidator.Setup(_ => _.IsValid("validId")).Returns(true);

                // Action
                var sut = new UrlRedirectController(mockStore.Object, mockValidator.Object);
                var actionResult = await sut.RedirectToLongUrl("validId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.IsType<NotFoundResult>(actionResult);
            }

            [Fact]
            public async Task WhenIdFound_ReturnRedirect()
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

                // Action
                var sut = new UrlRedirectController(mockStore.Object, mockValidator.Object);
                var actionResult = await sut.RedirectToLongUrl("validId");

                // Assert
                Assert.NotNull(actionResult);
                Assert.IsType<RedirectResult>(actionResult);
                Assert.Equal("http://mylongurl.com/abc/?id=zyx", ((RedirectResult)actionResult).Url);
            }
        }
    }
}


