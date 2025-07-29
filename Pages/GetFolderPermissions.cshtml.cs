using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Services;
using EgnyteApp.Models;

namespace EgnyteApp.Pages;

public class GetFolderPermissionsModel : PageModel
{
    private readonly EgnyteFolderService _folderService;
    
    public GetFolderPermissionsModel(EgnyteFolderService folderService)
    {
        _folderService = folderService;
    }

    [BindProperty]
    public string FolderPath { get; set; } = "/";
    public EgnyteFolderPermsResponse? Permissions { get; set; }
    public string? Error { get; set; }
    public EgnyteFsResponse? CurrentFolderContent { get; set; }

    public async Task<IActionResult> OnGetAsync(string? folderPath)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage("/Auth/ResourceOwnerLogin");

        var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Error = "No access token found. Please login again.";
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            FolderPath = folderPath;
        }

        try
        {
            CurrentFolderContent = await _folderService.ListFileOrFolderAsync(accessToken, FolderPath);
            Permissions = await _folderService.GetFolderPermissionsAsync(accessToken, FolderPath);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return Page();
    }
}
