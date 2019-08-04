using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UrlShortener.Common.Validation
{
    public class AliasValidator : IAliasValidator
    {

        private const string AliasRegexStr = "^[a-zA-Z0-9+\\-=]*$"; // TODO - Move
        private static Regex AliasRegex = new Regex(AliasRegexStr); // TODO - Move

        public bool IsValid(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return AliasRegex.IsMatch(id);
        }

    }
}
