using System.Text.Json.Serialization;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Models;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    [JsonPropertyName("userProfile")]
    public UserProfile? UserProfile { get; set; }
}