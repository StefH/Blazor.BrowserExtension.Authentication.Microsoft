using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public static class ChromeStorageLocalExtensions
{
    public static Task<string?> GetAccessTokenAsync(this ChromeStorageLocal storage)
    {
        return storage.GetSingleStringAsync("accessToken");
    }
}