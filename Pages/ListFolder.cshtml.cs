using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Models;
using EgnyteApp.Services;
using System.Text.Json;

namespace EgnyteApp.Pages;

public class ListFolderModel : PageModel
{
    private readonly EgnyteFolderService _folderService;

    public ListFolderModel(EgnyteFolderService folderService)
    {
        _folderService = folderService;
    }

    [BindProperty]
    public string FolderPath { get; set; } = "/Shared";
    public EgnyteFsResponse? FolderContent { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, string>? UserPermissions { get; set; }
    public Dictionary<string, string>? GroupPermissions { get; set; }
    public bool? InheritsPermissions { get; set; }
    public string? SelectedFolderPath { get; set; }

    public async Task<IActionResult> OnGetAsync(string? path, string? selectedFolder = null)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Auth/ResourceOwnerLogin");
        }
        
        if (!string.IsNullOrEmpty(path))
        {
            FolderPath = path;
        }

        var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (string.IsNullOrEmpty(accessToken))
        {
            Error = "No access token found. Please login again.";
            return Page();
        }

        try
        {
            FolderContent = await _folderService.ListFileOrFolderAsync(accessToken, FolderPath);
            
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                SelectedFolderPath = selectedFolder;
                var permissions = await _folderService.GetFolderPermissionsAsync(accessToken, selectedFolder);
                if (permissions != null)
                {
                    UserPermissions = permissions.UserPerms;
                    GroupPermissions = permissions.GroupPerms;
                    InheritsPermissions = permissions.InheritsPermissions;
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return new JsonResult(new { 
                userPerms = UserPermissions, 
                groupPerms = GroupPermissions, 
                inherits = InheritsPermissions 
            });
        }

        return Page();
    }
}
