using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Services;
using EgnyteApp.Models;
using System.Text.Json;

namespace EgnyteApp.Pages;

public class SetFolderPermissionModel : PageModel
{
    private readonly EgnyteFolderService _folderService;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<SetFolderPermissionModel> _logger;

    public SetFolderPermissionModel(
        EgnyteFolderService folderService,
        IConfiguration configuration,
        ILogger<SetFolderPermissionModel> logger)
    {
        _folderService = folderService;
        _configuration = configuration;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [BindProperty]
    public string FolderPath { get; set; } = "/";

    [BindProperty]
    public string GroupName { get; set; } = "";

    [BindProperty]
    public string Permission { get; set; } = "";

    public EgnyteFsResponse? CurrentFolderContent { get; set; }
    public string? Error { get; set; }
    public string? ApiRequest { get; set; }
    public string? ApiResponse { get; set; }
    public string? ApiUrl { get; set; }
    public bool Success { get; set; }

    public List<string> PermissionValues { get; } = new()
    {
        "None",
        "Viewer",
        "Editor",
        "Full",
        "Owner"
    };

    public async Task<IActionResult> OnGetAsync(string? path)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage("/Auth/ResourceOwnerLogin");

        var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Error = "No access token found. Please login again.";
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(path))
        {
            FolderPath = path;
        }

        try
        {
            CurrentFolderContent = await _folderService.ListFileOrFolderAsync(accessToken, FolderPath);
        }
        catch (Exception ex)
        {
            Error = "Unable to list folder contents: " + ex.Message;
            _logger.LogError(ex, "Error listing folder contents for path: {Path}", FolderPath);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage("/Auth/ResourceOwnerLogin");

        var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Error = "No access token found. Please login again.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(GroupName))
        {
            Error = "Group name is required.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Permission))
        {
            Error = "Permission is required.";
            return Page();
        }

        try
        {
            // Get folder content for display
            CurrentFolderContent = await _folderService.ListFileOrFolderAsync(accessToken, FolderPath);

            // Prepare API request preview
            var domain = _configuration["Egnyte:Domain"];
            var cleanPath = FolderPath.TrimStart('/');
            var encodedPath = cleanPath.Replace("!", "%21");
            ApiUrl = $"https://{domain}/pubapi/v2/perms/{encodedPath}";

            var requestBody = new
            {
                groupPerms = new Dictionary<string, string>
                {
                    { GroupName, Permission }
                },
                inheritsPermissions = false,
                keepParentPermissions = true
            };

            ApiRequest = JsonSerializer.Serialize(requestBody, _jsonOptions);

            try
            {
                // Set permissions
                await _folderService.SetFolderPermissionsAsync(
                    accessToken,
                    FolderPath,
                    groupPerms: new Dictionary<string, string> { { GroupName, Permission } }
                );

                Success = true;
                ApiResponse = JsonSerializer.Serialize(new { status = "success", message = "Permissions set successfully" }, _jsonOptions);
            }
            catch (Exception ex)
            {
                Success = false;
                ApiResponse = JsonSerializer.Serialize(new { status = "error", message = ex.Message }, _jsonOptions);
                Error = ex.Message;
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            _logger.LogError(ex, "Error setting permissions for path: {Path}", FolderPath);
        }

        return Page();
    }
}