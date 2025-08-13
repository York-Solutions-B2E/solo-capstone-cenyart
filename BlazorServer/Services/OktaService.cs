using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServer.Services;

[AllowAnonymous]
[Route("Account")]
public class AccountController : Controller
{
    // GET /Account/SignIn?returnUrl=/somewhere
    [HttpGet("SignIn")]
    public IActionResult SignIn(string? returnUrl = "/")
    {
        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        // Triggers the OIDC challenge and redirects to Okta (or configured provider)
        return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
    }

    // GET /Account/SignOut
    [HttpGet("SignOut")]
    public async Task<IActionResult> SignOutCallback()
    {
        // Sign out of the OIDC provider first, then clear the local cookie
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}

// AccessTokenHandler: attaches access token from HttpContext to outgoing HttpClient requests
public class AccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
        Console.WriteLine($"[AccessTokenHandler] Attaching token: {(token is null ? "null" : $"length {token.Length}")}");
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}


