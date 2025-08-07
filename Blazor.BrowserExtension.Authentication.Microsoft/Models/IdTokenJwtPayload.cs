using System.Text.Json.Serialization;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Models;

public class IdTokenJwtPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;


    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;


    [JsonPropertyName("upn")]
    public string? Upn { get; set; }


    [JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; set; }
}