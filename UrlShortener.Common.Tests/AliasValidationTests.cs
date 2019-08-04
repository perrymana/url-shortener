using System;
using UrlShortener.Common.Validation;
using Xunit;

namespace UrlShortener.Common.Tests
{
    public class AliasValidationTests
    {
        public class IsValid
        {

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("abc$xyz")]
            [InlineData("my/url")]
            public void WhenInvalidId_ReturnsFalse(string id)
            {
                var sut = new AliasValidator();

                var result = sut.IsValid(id);

                Assert.False(result);
            }

            [Theory]
            [InlineData("abcdefg")]
            [InlineData("abc+-123")]
            public void WhenValidId_ReturnsTrue(string id)
            {
                var sut = new AliasValidator();

                var result = sut.IsValid(id);

                Assert.True(result);
            }

        }

    }
}
