using System;
using System.Threading.Tasks;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public interface IChromeIdentity: IDisposable, IAsyncDisposable
{
    string? LaunchInteractiveWebAuthFlow(string url);

    Task<string?> LaunchInteractiveWebAuthFlowAsync(string url);

    string? GetRedirectUrl();

    Task<string?> GetRedirectUrlAsync();
}