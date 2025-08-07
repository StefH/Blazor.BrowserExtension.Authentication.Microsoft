using System.Globalization;
using System.Text.Json;
using Blazor.BrowserExtension.Authentication.Microsoft.Models;
using WebExtensions.Net.Storage;

// ReSharper disable once CheckNamespace
namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public static class StorageAreaExtensions
{
    public static async ValueTask<JsonElement?> GetPropertyAsync(this StorageArea storage, string key)
    {
        var result = await storage.Get(key);
        return result.TryGetProperty(key, out var property) ? property : null;
    }

    public static async ValueTask<T?> GetPropertyAsync<T>(this StorageArea storage, string key)
    {
        var result = await storage.GetPropertyAsync(key);
        return result != null ? result.Value.Deserialize<T>() : default;
    }

    public static async ValueTask<string?> GetStringAsync(this StorageArea storage, string key)
    {
        var result = await storage.GetPropertyAsync(key);
        return result?.GetString();
    }

    public static async ValueTask<long?> GetLongAsync(this StorageArea storage, string key)
    {
        var result = await storage.GetStringAsync(key);
        return long.TryParse(result, out var value) ? value : null;
    }

    internal static ValueTask<string?> GetAccessTokenAsync(this StorageArea storage)
    {
        return storage.GetStringAsync(nameof(TokenResponse.AccessToken));
    }

    internal static ValueTask StoreTokenAsync(this StorageArea storage, TokenResponse token)
    {
        return storage.Set(new Dictionary<string, object?>
        {
            { nameof(TokenResponse.TokenType), token.TokenType },
            { nameof(TokenResponse.AccessToken), token.AccessToken },
            { nameof(TokenResponse.RefreshToken), token.RefreshToken },
            { nameof(TokenResponse.ExpiresIn), (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + token.ExpiresIn * 1000).ToString(CultureInfo.InvariantCulture) },
            { nameof(TokenResponse.IdToken), token.IdToken },
            { nameof(TokenResponse.UserProfile), token.UserProfile }
        });
    }

    internal static ValueTask RemoveTokenAsync(this StorageArea storage)
    {
        return storage.Remove(
            new StorageAreaRemoveKeys(
            [
                nameof(TokenResponse.TokenType),
                nameof(TokenResponse.AccessToken),
                nameof(TokenResponse.RefreshToken),
                nameof(TokenResponse.ExpiresIn),
                nameof(TokenResponse.IdToken),
                nameof(TokenResponse.UserProfile)
            ]));
    }
}