using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TokenProvider.Infrastructure.Models;

namespace TokenProvider.Functions
{
    public class GenerateToken
    {
        private readonly ILogger<GenerateToken> _logger;

        public GenerateToken(ILogger<GenerateToken> logger)
        {
            _logger = logger;
        }

        [Function("GenerateToken")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/generate")] HttpRequest req, [FromBody] TokenRequest tokenRequest)
        {
            if (tokenRequest == null || tokenRequest.Email == null)
                return new BadRequestObjectResult(new { Error = "Please provide a valid user id and email."});

            return new OkObjectResult("");
        }
    }
}
