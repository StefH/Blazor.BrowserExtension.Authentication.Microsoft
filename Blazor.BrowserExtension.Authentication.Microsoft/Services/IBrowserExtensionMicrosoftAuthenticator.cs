using System.Threading.Tasks;
using Blazor.BrowserExtension.Authentication.Microsoft.Models;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Services;

public interface IBrowserExtensionMicrosoftAuthenticator
{
    Task<TokenResponse> AuthenticateAsync();

    Task<bool> IsAuthenticatedAsync();

    Task<UserProfile?> GetCurrentUserAsync();
}