﻿using System.Web;

namespace Sisk.Core.Http;

/// <summary>
/// Provides a class that contains useful methods for working with cookies in HTTP responses.
/// </summary>
/// <definition>
/// public abstract class CookieHelper
/// </definition>
/// <type>
/// Class
/// </type>
public abstract class CookieHelper
{
    internal abstract void SetCookieHeader(string name, string value);

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <definition>
    /// public void SetCookie(string name, string value)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public void SetCookie(string name, string value)
    {
        SetCookieHeader("Set-Cookie", $"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
    }

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expires">The cookie expirity date.</param>
    /// <param name="maxAge">The cookie max duration after being set.</param>
    /// <param name="domain">The domain where the cookie will be valid.</param>
    /// <param name="path">The path where the cookie will be valid.</param>
    /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
    /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
    /// <param name="sameSite">The cookie SameSite parameter.</param>
    /// <definition>
    /// public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly, string? sameSite)
    {
        List<string> syntax = new List<string>();
        syntax.Add($"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
        if (expires != null)
        {
            syntax.Add($"Expires={expires.Value.ToUniversalTime():r}");
        }
        if (maxAge != null)
        {
            syntax.Add($"Max-Age={maxAge.Value.TotalSeconds}");
        }
        if (domain != null)
        {
            syntax.Add($"Domain={domain}");
        }
        if (path != null)
        {
            syntax.Add($"Path={path}");
        }
        if (secure == true)
        {
            syntax.Add($"Secure");
        }
        if (httpOnly == true)
        {
            syntax.Add($"HttpOnly");
        }
        if (sameSite != null)
        {
            syntax.Add($"SameSite={sameSite}");
        }

        SetCookieHeader("Set-Cookie", String.Join("; ", syntax));
    }

}
