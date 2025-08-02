using System.Threading.Tasks;
using JsBind.Net;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

internal sealed class ChromeIdentity : ObjectBindingBase, IChromeIdentity
{
    public ChromeIdentity(IJsRuntimeAdapter jsRuntime)
    {
        SetAccessPath("chrome.identity");
        Initialize(jsRuntime);
    }

    public string? LaunchInteractiveWebAuthFlow(string url) => Invoke<string>("launchWebAuthFlow", new { url, interactive = true });

    public async Task<string?> LaunchInteractiveWebAuthFlowAsync(string url) => await InvokeAsync<string>("launchWebAuthFlow", new { url, interactive = true });

    public string? GetRedirectUrl() => Invoke<string>("getRedirectURL");

    public async Task<string?> GetRedirectUrlAsync() => await InvokeAsync<string>("getRedirectURL");
}