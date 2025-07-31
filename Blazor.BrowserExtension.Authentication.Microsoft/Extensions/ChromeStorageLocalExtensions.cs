// ReSharper disable once CheckNamespace
using System.Threading.Tasks;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public static class ChromeStorageLocalExtensions
{
    public static Task<string> GetAccessTokenAsync(this ChromeStorageLocal storage)
    {
        return storage.GetSingleStringAsync("accessToken");
    }
}