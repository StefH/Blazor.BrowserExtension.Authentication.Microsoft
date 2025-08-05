using Blazor.BrowserExtension.Authentication.Microsoft.Models;

// ReSharper disable once CheckNamespace
namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public static class ChromeLocalStorageExtensions
{
    public static ValueTask<string?> GetAccessTokenAsync(this IChromeStorageLocal storage)
    {
        return storage.GetSingleStringAsync("accessToken");
    }

    internal static ValueTask StoreTokenAsync(this IChromeStorageLocal storage, TokenResponse token)
    {
        return storage.SetAsync(new Dictionary<string, object?>
        {
            { nameof(TokenResponse.TokenType), token.TokenType },
            { nameof(TokenResponse.AccessToken), token.AccessToken },
            { nameof(TokenResponse.RefreshToken), token.RefreshToken },
            { nameof(TokenResponse.ExpiresIn), (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + token.ExpiresIn * 1000).ToString() },
            { nameof(TokenResponse.IdToken), token.IdToken },
            { nameof(TokenResponse.UserProfile), token.UserProfile }
        });
    }

    internal static ValueTask RemoveTokenAsync(this IChromeStorageLocal storage)
    {
        return storage.RemoveAsync(
            nameof(TokenResponse.TokenType),
            nameof(TokenResponse.AccessToken),
            nameof(TokenResponse.RefreshToken),
            nameof(TokenResponse.ExpiresIn),
            nameof(TokenResponse.IdToken),
            nameof(TokenResponse.UserProfile));
    }
}