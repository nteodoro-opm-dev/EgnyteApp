using System.Net.Http.Headers;
using System.Text.Json;
using EgnyteApp.Models;
using Microsoft.Extensions.Options;

namespace EgnyteApp.Auth;

public class EgnyteAuthService
{
    private readonly HttpClient _httpClient;
    private readonly EgnyteAuthOptions _options;

    public EgnyteAuthService(HttpClient httpClient, IOptions<EgnyteAuthOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string GetAuthorizationUrl(string domain, string state)
    {
        var endpoint = _options.AuthorizationEndpoint.Replace("{domain}", domain);
        var scopes = string.Join(" ", _options.Scopes);
        
        var url = $"{endpoint}?client_id={_options.ClientId}" +
                 $"&redirect_uri={Uri.EscapeDataString(_options.CallbackUrl)}" +
                 $"&scope={Uri.EscapeDataString(scopes)}" +
                 $"&state={Uri.EscapeDataString(state)}" +
                 "&response_type=code";
        
        return url;
    }

    public async Task<string> ExchangeCodeForTokenAsync(string domain, string code)
    {
        var endpoint = _options.TokenEndpoint.Replace("{domain}", domain);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.CallbackUrl,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
        
        return tokenResponse?.AccessToken ?? throw new Exception("Failed to get access token");
    }

    public async Task<string> ResourceOwnerPasswordLoginAsync(string username, string password)
    {
        var endpoint = _options.TokenEndpoint;
        var scopes = string.Join(" ", _options.Scopes);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password",
            ["scope"] = scopes
        });

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
        
        return tokenResponse?.AccessToken ?? throw new Exception("Failed to get access token");
    }

    public async Task<EgnyteUserInfo> GetUserInfoAsync(string domain, string accessToken)
    {
        var endpoint = _options.UserInfoEndpoint.Replace("{domain}", domain);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<EgnyteUserInfo>(jsonResponse);
        
        return userInfo ?? throw new Exception("Failed to get user info");
    }
}