using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using JsBind.Net;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public sealed class ChromeStorageLocal : ObjectBindingBase
{
    public ChromeStorageLocal(IJsRuntimeAdapter jsRuntime)
    {
        SetAccessPath("chrome.storage.local");
        Initialize(jsRuntime);
    }

    public Dictionary<string, object>? Get(string key) => Invoke<Dictionary<string, object>>("get", key);

    public object? GetSingle(string key) => Invoke<Dictionary<string, object>>("get", key)?[key];

    public Dictionary<string, object>? Get(string[] keys) => Invoke<Dictionary<string, object>>("get", keys);

    public Dictionary<string, object>? Get(Dictionary<string, object> keys) => Invoke<Dictionary<string, object>>("get", keys);

    public Dictionary<string, object>? GetAll() => Invoke<Dictionary<string, object>>("get", (object?)null);

    public void Set(Dictionary<string, object> items) => InvokeVoid("set", items);

    public void Set(string key, object value) => InvokeVoid("set", new Dictionary<string, object> { { key, value } });

    public void Remove(string key) => InvokeVoid("remove", key);

    public void Remove(string[] keys) => InvokeVoid("remove", keys);

    public void Clear() => InvokeVoid("clear");

    public int GetBytesInUse(string? key = null) => key == null ? Invoke<int>("getBytesInUse") : Invoke<int>("getBytesInUse", key);

    public int GetBytesInUse(string[] keys) => Invoke<int>("getBytesInUse", keys);

    public async Task<Dictionary<string, object>?> GetAsync(string key) => await InvokeAsync<Dictionary<string, object>>("get", key);

    public async Task<JsonElement> GetSingleAsync(string key)
    {
        var result = await InvokeAsync<Dictionary<string, object>>("get", key);
        return result != null && result.TryGetValue(key, out var value) && value is JsonElement element ? element : default;
    }

    public async Task<string> GetSingleStringAsync(string key)
    {
        var result = await GetSingleAsync(key);
        return result.GetString()!;
    }

    public async Task<Dictionary<string, object>?> GetAsync(string[] keys) => await InvokeAsync<Dictionary<string, object>>("get", keys);

    public async Task<Dictionary<string, object>?> GetAsync(Dictionary<string, object> keys) => await InvokeAsync<Dictionary<string, object>>("get", keys);

    public async Task<Dictionary<string, object>?> GetAllAsync() => await InvokeAsync<Dictionary<string, object>>("get", (object?)null);

    public async Task SetAsync(Dictionary<string, object> items) => await InvokeVoidAsync("set", items);

    public async Task SetAsync(string key, object value) => await InvokeVoidAsync("set", new Dictionary<string, object> { { key, value } });

    public async Task RemoveAsync(string key) => await InvokeVoidAsync("remove", key);

    public async Task RemoveAsync(string[] keys) => await InvokeVoidAsync("remove", keys);

    public async Task ClearAsync() => await InvokeVoidAsync("clear");

    public async Task<int> GetBytesInUseAsync(string? key = null) => key == null ? await InvokeAsync<int>("getBytesInUse") : await InvokeAsync<int>("getBytesInUse", key);

    public async Task<int> GetBytesInUseAsync(string[] keys) => await InvokeAsync<int>("getBytesInUse", keys);
}