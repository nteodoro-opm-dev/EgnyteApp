using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Services;
using EgnyteApp.Models;
using System.Text.RegularExpressions;

namespace EgnyteApp.Pages;

public partial class GetFolderPermissionsModel : PageModel
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

    [GeneratedRegex(@"(\d+)")]
    private static partial Regex NumberRegex();

    private static int CompareNatural(string? x, string? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var regex = NumberRegex();
        string[] xParts = regex.Split(x);
        string[] yParts = regex.Split(y);

        for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
        {
            // If both parts are numbers
            if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
            {
                if (xNum != yNum)
                    return xNum.CompareTo(yNum);
            }
            // If not numbers, compare as strings
            else
            {
                int result = string.Compare(xParts[i], yParts[i], StringComparison.CurrentCulture);
                if (result != 0)
                    return result;
            }
        }

        // If all parts are equal, shorter string comes first
        return xParts.Length.CompareTo(yParts.Length);
    }

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
            
            // Sort folders using natural sort
            if (CurrentFolderContent?.folders != null)
            {
                CurrentFolderContent.folders = CurrentFolderContent.folders
                    .OrderBy(f => f.name, Comparer<string?>.Create(CompareNatural))
                    .ToList();
            }

            Permissions = await _folderService.GetFolderPermissionsAsync(accessToken, FolderPath);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return Page();
    }
}
