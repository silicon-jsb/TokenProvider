using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Infrastructure.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken);
    }
}