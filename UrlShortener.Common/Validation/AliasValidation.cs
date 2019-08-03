using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UrlShortener.Common.Validation
{
    public static class AliasValidation
    {

        private const string AliasRegexStr = "^[a-zA-Z0-9+\\-=]*$"; // TODO - Move
        private static Regex AliasRegex = new Regex(AliasRegexStr); // TODO - Move

        public static bool IsValid(string id)
        {
            return AliasRegex.IsMatch(id);
        }

    }
}
