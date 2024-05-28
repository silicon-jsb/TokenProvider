using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net;
using TokenProvider.Infrastructure.Data.Contexts;
using TokenProvider.Infrastructure.Data.Entities;
using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Infrastructure.Services;

public class RefreshTokenService(IDbContextFactory<DataContext> dbContextFactory) : IRefreshTokenService
{
    private readonly IDbContextFactory<DataContext> _dbContextFactory = dbContextFactory;


    #region GetRefreshTokenAsync
    public async Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        RefreshTokenResult refreshTokenResult = null!;


        var refreshTokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.ExpiryDate > DateTime.Now, cancellationToken);
        if (refreshTokenEntity == null) 
        {
            refreshTokenResult = new RefreshTokenResult()
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Error = "Refresh token not found or expired."
            };
        }
        else
        {
            refreshTokenResult = new RefreshTokenResult()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Token = refreshTokenEntity!.RefreshToken,
                ExpiryDate = refreshTokenEntity.ExpiryDate,
                CookieOptions = CookieGenerator.GenerateCookie(refreshTokenEntity.ExpiryDate)
            };
        }
        return refreshTokenResult;
    }

    #endregion

    #region SaveRefreshTokenAsync
    public async Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var tokenLifeTime = double.TryParse(Environment.GetEnvironmentVariable("TOKEN_REFRESHTOKEN_LIFETIME"), out double refreshTokenLifeTime) ? refreshTokenLifeTime : 7;

            await using var context = _dbContextFactory.CreateDbContext();
            var refreshTokenEntity = new RefreshTokenEntity()
            {
                RefreshToken = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddDays(tokenLifeTime),
            };
            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
        
    }

    #endregion
}
