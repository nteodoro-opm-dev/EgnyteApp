using System.Text.Json.Serialization;

namespace EgnyteApp.Models;

public class EgnyteFsResponse
{
    public string? name { get; set; }
    public string? path { get; set; }
    public bool is_folder { get; set; }
    public List<EgnyteFsFolder>? folders { get; set; }
    public List<EgnyteFsFile>? files { get; set; }
    public int? total_count { get; set; }
}

public class EgnyteFsFolder
{
    public string? name { get; set; }
    public string? path { get; set; }
    public bool is_folder { get; set; }
}

public class EgnyteFsFile
{
    public string? name { get; set; }
    public string? path { get; set; }
    public bool is_folder { get; set; }
    public long? size { get; set; }
    public string? uploaded_by { get; set; }
    public string? last_modified { get; set; }
}

public class EgnyteFolderPermsResponse
{
    [JsonPropertyName("userPerms")]
    public Dictionary<string, string>? UserPerms { get; set; }
    [JsonPropertyName("groupPerms")]
    public Dictionary<string, string>? GroupPerms { get; set; }
    [JsonPropertyName("inheritsPermissions")]
    public bool InheritsPermissions { get; set; }
}

internal class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}