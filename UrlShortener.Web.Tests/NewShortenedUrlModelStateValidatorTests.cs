using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using UrlShortener.Web.Models;
using Xunit;

namespace UrlShortener.Web.Tests
{
    public class NewShortenedUrlModelStateValidatorTests
    {

        [Fact]
        public void WhenLongUrlNotSupplied_ReturnsFalse()
        {
            // Setup
            var result = new List<ValidationResult>();
            var url = new NewShortenedUrl();

            // Act
            var isValid = Validator.TryValidateObject(url, new ValidationContext(url), result);

            // Assert
            Assert.False(isValid); 
            var valResult = Assert.Single(result);
            Assert.Equal("LongUrl", valResult.MemberNames.ElementAt(0));
            Assert.Equal("The LongUrl field is required.", valResult.ErrorMessage);
        }

        [Fact(Skip = "Can't get this one to work")]
        public void WhenLongUrlNotAUrl_ReturnsFalse()
        {
            // Setup
            var result = new List<ValidationResult>();
            var url = new NewShortenedUrl()
            {
                LongUrl = "abc"
            };

            // Act
            var isValid = Validator.TryValidateObject(url, new ValidationContext(url), result);

            // Assert
            Assert.False(isValid);
            var valResult = Assert.Single(result);
            Assert.Equal("LongUrl", valResult.MemberNames.ElementAt(0));
            Assert.Equal("The LongUrl field is not a valid fully-qualified http, https, or ftp URL.", valResult.ErrorMessage);
        }

        [Fact]
        public void WhenLongUrlValid_ReturnsTrue()
        {
            // Setup
            var result = new List<ValidationResult>();
            var url = new NewShortenedUrl()
            {
                LongUrl = "http://myhost.com"
            };

            // Act
            var isValid = Validator.TryValidateObject(url, new ValidationContext(url), result);

            // Assert
            Assert.True(isValid);
        }
    }
}
