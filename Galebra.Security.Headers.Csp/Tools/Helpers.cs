using System;
using System.Collections.Generic;
using System.Linq;

namespace Galebra.Security.Headers.Csp.Tools
{ 

internal static class Helpers
{
    /// <summary>
    /// Converts to <see cref="string.Empty"/> a null, empty or whitespace string. Otherwise, it trims.<br/>
    /// This method solves the folowwing problem:<br/>
    /// Null, empty or white spaces are accepted as CSP header values by both the .Net runtime and browsers.<br/>
    /// However, having white spaces in the header values is not a got idea because the server sends<br/>
    /// the csp header name with a blank value, whereas no header will be sent with string.Empty
    /// (if an additional nonce is not required)
    /// </summary>
    /// <param name="data"></param>
    internal static string TrimEmptyfy(this string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            data = string.Empty;
        }
        return data.Trim();
    }
    /// <summary>
    /// Removes all null and white spaces in the list of strings<br/>
    /// and trims and emptifies all strings by calling <seealso cref="TrimEmptyfy(string)"/>
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    internal static IList<string> RemoveAllTrimEmptyfy(this List<string> list)
    {
        list.RemoveAll(x => string.IsNullOrWhiteSpace(x));
        return list.Select(x => x.TrimEmptyfy()).ToList();
    }

    /// <summary>
    /// Returns true if an <see cref="IList"/> of string :<br/>
    /// Has at least one string item which is null, empty or white space<br/>
    /// Notice that this method returns false when the list has not item
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    internal static bool HasNullOrWhiteSpace(this IList<string> list)
    {
        return list.Any(s => string.IsNullOrWhiteSpace(s));
    }

    /// <summary>
    /// Quick check on format. Currently checks for trailing semicolons and throws if found
    /// </summary>
    /// <param name="data"></param>
    /// <param name="info"></param>
    internal static void CheckData(string data, string? info = null)
    {
        if (!string.IsNullOrWhiteSpace(data)
            && (data.EndsWith(';') || data.StartsWith(';')))
        {
            throw new Exception($"Value cannot start or end with a semicolon: " +
                $"{info} {data}");
        }
    }

    /// <summary>
    /// <inheritdoc cref="CheckData(string, string?)"/>
    /// </summary>
    /// <param name="nonceable"></param>
    internal static void CheckData(IList<string> nonceable, string? info = null)
    {
        foreach (var item in nonceable)
        {
            CheckData(item, info);
        }
    }
}

}
