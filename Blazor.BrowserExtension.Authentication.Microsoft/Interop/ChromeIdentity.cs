using JsBind.Net;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

internal sealed class ChromeIdentity : ObjectBindingBase, IChromeIdentity
{
    public ChromeIdentity(IJsRuntimeAdapter jsRuntime)
    {
        SetAccessPath("chrome.identity");
        Initialize(jsRuntime);
    }

    public Uri? LaunchInteractiveWebAuthFlow(Uri url) => ToUri(Invoke<string>("launchWebAuthFlow", new { url, interactive = true }));

    public async ValueTask<Uri?> LaunchInteractiveWebAuthFlowAsync(Uri url)
    {
        var responseUrl = await InvokeAsync<string>("launchWebAuthFlow", new { url, interactive = true });
        return ToUri(responseUrl);
    }

    public string? GetRedirectUrl() => Invoke<string>("getRedirectURL");

    public ValueTask<string?> GetRedirectUrlAsync() => InvokeAsync<string>("getRedirectURL");

    private static Uri? ToUri(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) ? new Uri(url) : null;
    }
}