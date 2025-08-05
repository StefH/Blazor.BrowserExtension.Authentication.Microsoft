using System.Text.Json.Serialization;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Models;

public class UserProfile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("userPrincipalName")]
    public string? UserPrincipalName { get; set; }
}