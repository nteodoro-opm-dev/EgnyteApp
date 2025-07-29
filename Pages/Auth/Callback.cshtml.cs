using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Auth;

namespace EgnyteApp.Pages.Auth;

public class CallbackModel : PageModel
{
    private readonly EgnyteAuthService _authService;

    public string? Error { get; set; }

    public CallbackModel(EgnyteAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnGetAsync(string? code, string? state, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Error = $"Authentication failed: {error}";
            return Page();
        }

        var storedState = HttpContext.Session.GetString("OAuthState");
        var domain = HttpContext.Session.GetString("EgnyteDomain");

        if (string.IsNullOrEmpty(storedState) || string.IsNullOrEmpty(domain) || state != storedState)
        {
            Error = "Invalid state parameter. The request may have been tampered with.";
            return Page();
        }

        try
        {
            var accessToken = await _authService.ExchangeCodeForTokenAsync(domain, code ?? "");
            var userInfo = await _authService.GetUserInfoAsync(domain, accessToken);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                new(ClaimTypes.Name, userInfo.Username),
                new(ClaimTypes.GivenName, userInfo.FirstName),
                new(ClaimTypes.Surname, userInfo.LastName),
                new("egnyte_domain", domain),
                new("access_token", accessToken)
            };

            var identity = new ClaimsIdentity(claims, "Egnyte");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            Error = $"Failed to process authentication: {ex.Message}";
            return Page();
        }
    }
}