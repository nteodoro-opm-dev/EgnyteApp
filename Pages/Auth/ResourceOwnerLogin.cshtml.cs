using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Auth;

namespace EgnyteApp.Pages.Auth;

public class ResourceOwnerLoginModel : PageModel
{
    private readonly EgnyteAuthService _authService;
    private readonly EgnyteAuthOptions _options;

    public ResourceOwnerLoginModel(EgnyteAuthService authService, Microsoft.Extensions.Options.IOptions<EgnyteAuthOptions> options)
    {
        _authService = authService;
        _options = options.Value;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Username and password are required.";
            return Page();
        }
        try
        {
            var accessToken = await _authService.ResourceOwnerPasswordLoginAsync(Username, Password);
            // For resource owner flow, the domain is fixed in config
            var domain = _options.UserInfoEndpoint.Split('.')[0].Replace("https://", "");
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
            Error = ex.Message;
            return Page();
        }
    }
}
