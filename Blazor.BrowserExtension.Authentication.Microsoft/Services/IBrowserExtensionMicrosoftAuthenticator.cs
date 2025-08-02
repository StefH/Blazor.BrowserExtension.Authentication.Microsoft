using System.Threading.Tasks;
using Blazor.BrowserExtension.Authentication.Microsoft.Models;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Services;

public interface IBrowserExtensionMicrosoftAuthenticator
{
    Task<TokenResponse> AuthenticateAsync();

    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Retrieves the Access Token from the Chrome Local Storage for authenticating requests.
    /// In case the token is not available or expired, it will return null.
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    Task<UserProfile?> GetCurrentUserAsync();
}