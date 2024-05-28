using Microsoft.AspNetCore.Http;

namespace TokenProvider.Infrastructure.Services;

public static class CookieGenerator
{
    public static CookieOptions GenerateCookie(DateTimeOffset expireyDate)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = expireyDate
        };

        return cookieOptions;
    }
}
