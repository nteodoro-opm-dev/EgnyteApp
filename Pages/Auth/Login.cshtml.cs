using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Auth;

namespace EgnyteApp.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly EgnyteAuthService _authService;

    [BindProperty]
    public string Domain { get; set; } = string.Empty;

    public LoginModel(EgnyteAuthService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Domain))
        {
            ModelState.AddModelError("Domain", "Domain is required");
            return Page();
        }

        // Generate a random state value for CSRF protection
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        HttpContext.Session.SetString("OAuthState", state);
        HttpContext.Session.SetString("EgnyteDomain", Domain);

        var authorizationUrl = _authService.GetAuthorizationUrl(Domain, state);
        return Redirect(authorizationUrl);
    }
}