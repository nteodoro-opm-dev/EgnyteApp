using System.Net.Http.Headers;
using System.Text.Json;
using EgnyteApp.Models;
using Microsoft.Extensions.Options;

namespace EgnyteApp.Services;

public class EgnyteFolderService
{
    private readonly HttpClient _httpClient;
    private readonly string _domain;
    private readonly ILogger<EgnyteFolderService> _logger;

    public EgnyteFolderService(HttpClient httpClient, IConfiguration configuration, ILogger<EgnyteFolderService> logger)
    {
        _httpClient = httpClient;
        _domain = configuration["Egnyte:Domain"] ?? "oceanpm.egnyte.com";
        _logger = logger;
    }

    public string GetDomain() => _domain;

    public async Task<EgnyteFsResponse?> ListFileOrFolderAsync(string accessToken, string path, bool listContent = true)
    {
        var endpoint = $"https://{_domain}/pubapi/v1/fs{path}";
        var uri = $"{endpoint}?list_content={listContent.ToString().ToLower()}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EgnyteFsResponse>(json);
    }

    public async Task SetFolderPermissionsAsync(string accessToken, string folderPath, string groupName, string permission)
    {
        var cleanPath = folderPath.TrimStart('/');
        var encodedPath = cleanPath.Replace("!", "%21");
        var endpoint = $"https://{_domain}/pubapi/v2/perms/{encodedPath}";

        try
        {
            // First check if we have permission to modify this folder
            var currentPerms = await GetFolderPermissionsAsync(accessToken, folderPath);
            
            var content = JsonSerializer.Serialize(new
            {
                groupPerms = new Dictionary<string, string>
                {
                    { groupName, permission }
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Forbidden access to folder: {Folder}. Response: {Response}", 
                        folderPath, errorContent);
                    throw new Exception($"You don't have permission to modify access rights for folder: {folderPath}. Please verify you have Owner or Full permissions.");
                }
                
                response.EnsureSuccessStatusCode(); // This will throw for other status codes
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error setting permissions for folder: {Folder}", folderPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting permissions for folder: {Folder}", folderPath);
            throw;
        }
    }

    public async Task<EgnyteFolderPermsResponse?> GetFolderPermissionsAsync(string accessToken, string folderPath)
    {
        var cleanPath = folderPath.TrimStart('/');
        var encodedPath = cleanPath.Replace("!", "%21");
        var endpoint = $"https://{_domain}/pubapi/v2/perms/{encodedPath}";
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Forbidden access to get permissions for folder: {Folder}. Response: {Response}", 
                folderPath, errorContent);
            throw new Exception($"You don't have permission to view permissions for folder: {folderPath}");
        }
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EgnyteFolderPermsResponse>(json);
    }

    public async Task SetFolderPermissionsAsync(
        string accessToken, 
        string folderPath,
        Dictionary<string, string>? userPerms = null,
        Dictionary<string, string>? groupPerms = null,
        bool? inheritsPermissions = null,
        bool? keepParentPermissions = null)
    {
        try
        {
            var cleanPath = folderPath.TrimStart('/');
            var encodedPath = cleanPath.Replace("!", "%21");
            var endpoint = $"https://{_domain}/pubapi/v2/perms/{encodedPath}";

            var requestBody = new Dictionary<string, object>();

            if (userPerms?.Count > 0)
                requestBody["userPerms"] = userPerms;
            if (groupPerms?.Count > 0)
                requestBody["groupPerms"] = groupPerms;
            if (inheritsPermissions.HasValue)
                requestBody["inheritsPermissions"] = inheritsPermissions.Value;
            if (keepParentPermissions.HasValue)
                requestBody["keepParentPermissions"] = keepParentPermissions.Value;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody, jsonOptions), 
                    System.Text.Encoding.UTF8, 
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API error: {StatusCode}, Response: {Response}", 
                    response.StatusCode, responseContent);
                throw new Exception($"Failed to set permissions: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SetFolderPermissionsAsync for path: {Path}", folderPath);
            throw;
        }
    }

    public async Task SetFolderPermissionsIfNeededAsync(string accessToken, string folderPath, string groupName, string permission)
    {
        var perms = await GetFolderPermissionsAsync(accessToken, folderPath);

        // Only set if group is missing or permission is different
        var groupPerms = perms?.GroupPerms ?? new Dictionary<string, string>();
        if (!groupPerms.TryGetValue(groupName, out var currentPerm) || !string.Equals(currentPerm, permission, StringComparison.OrdinalIgnoreCase))
        {
            await SetFolderPermissionsAsync(accessToken, folderPath, groupName, permission);
        }
    }

    public async Task<List<string>> GetSubFolderPathsAsync(string accessToken, string folderPath)
    {
        var subFolderPaths = new List<string>();
        try
        {
            var currentFolderContent = await ListFileOrFolderAsync(accessToken, folderPath);
            if (currentFolderContent?.folders != null)
            {
                subFolderPaths = currentFolderContent.folders
                    .Select(f => f.path ?? string.Empty)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSubFolderPathsAsync for folder: {FolderPath}", folderPath);
            throw;
        }
        return subFolderPaths;
    }
}