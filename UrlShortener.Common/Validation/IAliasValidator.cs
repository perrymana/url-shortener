using System;
using System.Collections.Generic;
using System.Text;

namespace UrlShortener.Common.Validation
{
    public interface IAliasValidator
    {
        bool IsValid(string id);
    }
}
