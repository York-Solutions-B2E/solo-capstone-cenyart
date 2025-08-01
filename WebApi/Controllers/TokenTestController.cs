using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenTestController(ITokenTestService tokenService, ILogger<TokenTestController> logger) : ControllerBase
{
    private readonly ITokenTestService _tokenService = tokenService;
    private readonly ILogger<TokenTestController> _logger = logger;

    [HttpPost("get-client-token")]
    public async Task<IActionResult> GetClientCredentialsToken()
    {
        try
        {
            var oktaDomain = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Okta:OktaDomain"];
            var clientId = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Okta:ClientId"];
            var clientSecret = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Okta:ClientSecret"];
            var authServerId = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Okta:AuthorizationServerId"] ?? "default";

            if (string.IsNullOrEmpty(oktaDomain) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Okta configuration is missing",
                    Details = new
                    {
                        HasOktaDomain = !string.IsNullOrEmpty(oktaDomain),
                        HasClientId = !string.IsNullOrEmpty(clientId),
                        HasClientSecret = !string.IsNullOrEmpty(clientSecret),
                        AuthServerId = authServerId
                    }
                });
            }

            var tokenResponse = await _tokenService.GetClientCredentialsTokenAsync();

            if (tokenResponse?.AccessToken != null)
            {
                return Ok(tokenResponse.AccessToken);
            }

            return BadRequest(new
            {
                Success = false,
                Message = "Failed to obtain token from Okta - check logs for details",
                Configuration = new
                {
                    TokenEndpoint = $"{oktaDomain}/oauth2/{authServerId}/v1/token",
                    ClientId = clientId,
                    HasClientSecret = !string.IsNullOrEmpty(clientSecret)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client credentials token");
            return StatusCode(500, new { Success = false, Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [Authorize]
    [HttpGet("test-protected")]
    public IActionResult TestProtectedEndpoint()
    {
        var user = HttpContext.User;

        return Ok(new
        {
            Success = true,
            Message = "🎉 Your JWT token is working! This is a protected endpoint.",
            UserInfo = new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Name = user.Identity?.Name,
                UserId = user.FindFirst("sub")?.Value,
                ClientId = user.FindFirst("cid")?.Value,
                Scopes = user.FindFirst("scp")?.Value?.Split(' ') ?? Array.Empty<string>(),
                Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
            },
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("get-curl-command")]
    public IActionResult GetCurlCommand()
    {
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var oktaDomain = config["Okta:OktaDomain"];
        var clientId = config["Okta:ClientId"];
        var clientSecret = config["Okta:ClientSecret"];
        var authServerId = config["Okta:AuthorizationServerId"] ?? "default";

        var curlCommand = $"""
        curl -X POST "{oktaDomain}/oauth2/{authServerId}/v1/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&scope=api://default"
        """;
        return Ok(curlCommand);
    }

}
