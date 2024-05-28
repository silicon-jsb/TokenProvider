using System.Security.Claims;
using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Infrastructure.Services
{
    public interface ITokenGenerator
    {
        AccessTokenResult GenerateAccessToken(TokenRequest tokenRequest, string? refreshToken);
        Task<RefreshTokenResult> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken);
    }
}