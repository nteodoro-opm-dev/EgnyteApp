using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgnyteApp.Models;
using EgnyteApp.Services;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace EgnyteApp.Pages;

public partial class BulkPermissionsModel : PageModel
{
    private readonly EgnyteFolderService _folderService;
    private readonly ILogger<BulkPermissionsModel> _logger;

    public BulkPermissionsModel(
        EgnyteFolderService folderService,
        ILogger<BulkPermissionsModel> logger)
    {
        _folderService = folderService;
        _logger = logger;
    }

    [BindProperty]
    public string FolderPath { get; set; } = "/";

    [BindProperty]
    public string GroupName { get; set; } = string.Empty;

    [BindProperty]
    public List<string> SubFolders { get; set; } = new();

    [BindProperty]
    public List<string> Permissions { get; set; } = new();

    public List<string> PermissionValues { get; } = new()
    {
        "",
        "None",
        "Viewer Only",
        "Viewer",
        "Editor",
        "Full",
        "Owner"
    };

    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public EgnyteFsResponse? CurrentFolderContent { get; set; }
    public List<ApiDebugInfo> DebugInfo { get; private set; } = new();

    [TempData]
    public string? ProcessStatus { get; set; }

    public class ApiDebugInfo
    {
        public string Operation { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string Request { get; set; } = "";
        public string Response { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FolderPath { get; set; }
    }

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
            var debugInfo = new ApiDebugInfo
            {
                Operation = "List Folder Contents",
                Endpoint = $"https://{_folderService.GetDomain()}/pubapi/v1/fs{FolderPath}",
                Request = "GET Request with Authorization header",
                Timestamp = DateTime.UtcNow,
                FolderPath = FolderPath
            };

            try
            {
                CurrentFolderContent = await _folderService.ListFileOrFolderAsync(accessToken, FolderPath);
                debugInfo.IsSuccess = true;
                debugInfo.Response = JsonSerializer.Serialize(CurrentFolderContent, new JsonSerializerOptions { WriteIndented = true });

                // Sort folders using natural sort
                if (CurrentFolderContent?.folders != null)
                {
                    CurrentFolderContent.folders = CurrentFolderContent.folders
                        .OrderBy(f => f.name, Comparer<string?>.Create(CompareNatural))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                debugInfo.IsSuccess = false;
                debugInfo.ErrorMessage = ex.Message;
                throw;
            }
            finally
            {
                DebugInfo.Add(debugInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading folder contents for path: {Path}", FolderPath);
            Error = "Unable to list folder contents: " + ex.Message;
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
            return await OnGetAsync(FolderPath);
        }

        try
        {
            var successCount = 0;
            var failureCount = 0;
            var failures = new List<(string Path, string Error)>();
            var totalRequests = SubFolders
                .Where((_, i) => i < Permissions.Count && !string.IsNullOrEmpty(Permissions[i]))
                .Count();
            var currentRequest = 0;

            ProcessStatus = "Starting permission updates...";
            await Task.Delay(500); // Small delay to ensure UI updates

            for (int i = 0; i < SubFolders.Count && i < Permissions.Count; i++)
            {
                if (string.IsNullOrEmpty(Permissions[i]))
                {
                    continue;
                }

                currentRequest++;
                ProcessStatus = $"Processing {currentRequest} of {totalRequests}: {Path.GetFileName(SubFolders[i])}";

                var debugInfo = new ApiDebugInfo
                {
                    Operation = "Set Folder Permissions",
                    Endpoint = $"https://{_folderService.GetDomain()}/pubapi/v2/perms/{SubFolders[i].TrimStart('/').Replace("!", "%21")}",
                    Request = JsonSerializer.Serialize(
                        new { groupPerms = new Dictionary<string, string> { { GroupName, Permissions[i] } } },
                        new JsonSerializerOptions { WriteIndented = true }
                    ),
                    Timestamp = DateTime.UtcNow,
                    FolderPath = SubFolders[i]
                };

                try
                {
                    await _folderService.SetFolderPermissionsAsync(accessToken, SubFolders[i], GroupName, Permissions[i]);
                    debugInfo.IsSuccess = true;
                    debugInfo.Response = "Success (No content returned)";
                    successCount++;
                    
                    // Add a small delay between requests to prevent overwhelming the API
                    await Task.Delay(250);
                }
                catch (Exception ex)
                {
                    debugInfo.IsSuccess = false;
                    debugInfo.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Error setting permissions for folder: {Folder}", SubFolders[i]);
                    failures.Add((SubFolders[i], ex.Message));
                    failureCount++;
                    
                    // Add a longer delay after errors
                    await Task.Delay(500);
                }
                finally
                {
                    DebugInfo.Add(debugInfo);
                }
            }

            ProcessStatus = "Finalizing...";
            await Task.Delay(500);

            if (successCount > 0)
            {
                SuccessMessage = $"Successfully updated permissions for {successCount} subfolder(s).";
            }

            if (failureCount > 0)
            {
                Errors = failures.Select(f => $"Failed to set permissions for '{f.Path}': {f.Error}").ToList();
            }

            ProcessStatus = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk permission update");
            Error = "Error updating permissions: " + ex.Message;
            ProcessStatus = null;
        }

        return await OnGetAsync(FolderPath);
    }

    public async Task<IActionResult> OnPostUpdatePermissionAsync([FromBody] UpdatePermissionRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken))
            return new JsonResult(new { error = "No access token found. Please login again." }) { StatusCode = 401 };

        if (string.IsNullOrWhiteSpace(request.GroupName))
            return BadRequest(new { error = "Group name is required." });

        if (string.IsNullOrWhiteSpace(request.Permission))
            return BadRequest(new { error = "Permission is required." });

        var debugInfo = new ApiDebugInfo
        {
            Operation = "Set Folder Permission",
            Endpoint = $"https://{_folderService.GetDomain()}/pubapi/v2/perms/{request.FolderPath.TrimStart('/').Replace("!", "%21")}",
            Request = JsonSerializer.Serialize(
                new { groupPerms = new Dictionary<string, string> { { request.GroupName, request.Permission } } },
                new JsonSerializerOptions { WriteIndented = true }
            ),
            Timestamp = DateTime.UtcNow,
            FolderPath = request.FolderPath
        };

        try
        {
            await _folderService.SetFolderPermissionsAsync(accessToken, request.FolderPath, request.GroupName, request.Permission);
            debugInfo.IsSuccess = true;
            debugInfo.Response = "Success (No content returned)";
            DebugInfo.Add(debugInfo);

            return new JsonResult(new { 
                success = true,
                debugHtml = RenderDebugInfo(debugInfo)
            });
        }
        catch (Exception ex)
        {
            debugInfo.IsSuccess = false;
            debugInfo.ErrorMessage = ex.Message;
            DebugInfo.Add(debugInfo);
            _logger.LogError(ex, "Error setting permissions for folder: {Folder}", request.FolderPath);
            
            return new JsonResult(new { 
                success = false, 
                error = ex.Message,
                debugHtml = RenderDebugInfo(debugInfo)
            });
        }
    }

    private string RenderDebugInfo(ApiDebugInfo debug)
    {
        return $@"
        <div class=""card mb-3 {(debug.IsSuccess ? "border-success" : "border-danger")}"">
            <div class=""card-header bg-transparent {(debug.IsSuccess ? "border-success" : "border-danger")}"">
                <div class=""d-flex justify-content-between align-items-center"">
                    <strong>{debug.Operation}</strong>
                    <span class=""text-muted"">{debug.Timestamp:yyyy-MM-dd HH:mm:ss}</span>
                </div>
            </div>
            <div class=""card-body"">
                <h6 class=""card-subtitle mb-2"">Endpoint</h6>
                <pre class=""bg-light p-2 rounded""><code>{debug.Endpoint}</code></pre>

                <h6 class=""card-subtitle mb-2 mt-3"">Request</h6>
                <pre class=""bg-light p-2 rounded""><code>{debug.Request}</code></pre>

                <h6 class=""card-subtitle mb-2 mt-3"">Response</h6>
                <pre class=""bg-light p-2 rounded""><code>{(debug.IsSuccess ? debug.Response : debug.ErrorMessage)}</code></pre>
            </div>
        </div>";
    }

    public class UpdatePermissionRequest
    {
        public string FolderPath { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }
}