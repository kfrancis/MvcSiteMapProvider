using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Web.Html;

/// <summary>
///     Parses simple key=value pairs from a string to a dictionary, supporting separators ' ', ',', ';'.
///     Example: "id=siteMapLogoutLink class=btn;data-x=1".
/// </summary>
public static class HtmlAttributeParser
{
    public static IDictionary<string, object> Parse(string? input)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(input))
        {
            return dict;
        }

        foreach (var part in input!.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split(new[] { '=' }, 2);
            if (kv.Length != 2)
            {
                continue;
            }

            var key = kv[0].Trim();
            var val = kv[1].Trim();
            if (key.Length > 0)
            {
                dict[key] = val;
            }
        }

        return dict;
    }
}
