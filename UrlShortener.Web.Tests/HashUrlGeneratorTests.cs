using System;
using System.Collections.Generic;
using System.Text;
using UrlShortener.Web.Services;
using Xunit;

namespace UrlShortener.Web.Tests
{
    public class HashUrlGeneratorTests
    {

        public class GenerateNewId
        {

            [Fact]
            public void Test()
            {
                // Setup
                var sut = new HashUrlGenerator();

                // Act
                var newID = sut.GenerateNewId();

                // Assert
                // All we can really do here is make sure it didn't error and that the length is 8 characters.
                Assert.NotNull(newID);
                Assert.Equal(8, newID.Length);
            }

        }

    }
}
