namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public interface IChromeIdentity : IDisposable, IAsyncDisposable
{
    Uri? LaunchInteractiveWebAuthFlow(Uri url);

    ValueTask<Uri?> LaunchInteractiveWebAuthFlowAsync(Uri url);

    string? GetRedirectUrl();

    ValueTask<string?> GetRedirectUrlAsync();
}