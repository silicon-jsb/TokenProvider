using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TokenProvider.Infrastructure.Models;
using TokenProvider.Infrastructure.Services;

namespace TokenProvider.Functions
{
    public class RefreshTokens(ILogger<GenerateToken> logger, IRefreshTokenService refreshTokenService, ITokenGenerator tokenGenerator)
    {
        private readonly ILogger<GenerateToken> _logger = logger;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

        [Function("RefreshTokens")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/refresh")] HttpRequest req)
        {

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);

            if (tokenRequest == null || tokenRequest.Email == null || tokenRequest.UserId == null)
                return new BadRequestObjectResult(new { Error = "Please provide a valid user id and email." });

            try
            {
                RefreshTokenResult refreshTokenResult = null!;
                AccessTokenResult accessTokenResult = null!;

                using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeout.Token, req.HttpContext.RequestAborted);

                req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
                
                if (string.IsNullOrEmpty(refreshToken))
                    return new UnauthorizedObjectResult(new { Error = "Refresh token was not found." });

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    refreshTokenResult = await _refreshTokenService.GetRefreshTokenAsync(refreshToken, cts.Token);

                    if (refreshTokenResult.ExpiryDate < DateTime.Now)
                        return new UnauthorizedObjectResult(new { Error = "Refresh token has expired." });

                    if (refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                        refreshTokenResult = await _tokenGenerator.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

                    accessTokenResult = _tokenGenerator.GenerateAccessToken(tokenRequest, refreshTokenResult.Token);

                    if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null && refreshTokenResult.CookieOptions != null)
                    {
                        req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);
                        return new ObjectResult(new { AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token });
                    }
                }
   
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { Error = "An unexpected error occured while generating tokens" }) { StatusCode = 500 };
            }
            return new ObjectResult(new { Error = "Failed to generate tokens" }) { StatusCode = 500 };
        }
    }
}
