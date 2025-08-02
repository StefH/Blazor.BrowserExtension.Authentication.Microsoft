using System.Text.Json;
using System.Threading.Tasks;
using JsBind.Net;
using StorageData = System.Collections.Generic.Dictionary<string, object?>;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

internal sealed class ChromeStorageLocal : ObjectBindingBase, IChromeStorageLocal
{
    public ChromeStorageLocal(IJsRuntimeAdapter jsRuntime)
    {
        SetAccessPath("chrome.storage.local");
        Initialize(jsRuntime);
    }

    public StorageData? Get(string key) => Invoke<StorageData>("get", key);

    public JsonElement? GetSingle(string key)
    {
        var result = Get(key);
        return result != null && result.TryGetValue(key, out var value) && value is JsonElement element ? element : null;
    }

    public string? GetSingleString(string key)
    {
        var result = GetSingle(key);
        return result?.GetString();
    }

    public StorageData? Get(params string[] keys) => Invoke<StorageData>("get", keys);

    public StorageData? Get(StorageData keys) => Invoke<StorageData>("get", keys);

    public StorageData? GetAll() => Invoke<StorageData>("get", (object?)null);

    public void Set(StorageData items) => InvokeVoid("set", items);

    public void Set(string key, object? value) => InvokeVoid("set", new StorageData { { key, value } });

    public void Remove(string key) => InvokeVoid("remove", key);

    public void Remove(string[] keys) => InvokeVoid("remove", keys);

    public void Clear() => InvokeVoid("clear");

    public int GetBytesInUse(string? key = null) => key == null ? Invoke<int>("getBytesInUse") : Invoke<int>("getBytesInUse", key);

    public int GetBytesInUse(params string[] keys) => Invoke<int>("getBytesInUse", keys);

    public async Task<StorageData?> GetAsync(string key) => await InvokeAsync<StorageData>("get", key);

    public async Task<JsonElement?> GetSingleAsync(string key)
    {
        var result = await GetAsync(key);
        return result != null && result.TryGetValue(key, out var value) && value is JsonElement element ? element : null;
    }

    public async Task<string?> GetSingleStringAsync(string key)
    {
        var result = await GetSingleAsync(key);
        return result?.GetString();
    }

    public async Task<StorageData?> GetAsync(params string[] keys) => await InvokeAsync<StorageData>("get", keys);

    public async Task<StorageData?> GetAsync(StorageData keys) => await InvokeAsync<StorageData>("get", keys);

    public async Task<StorageData?> GetAllAsync() => await InvokeAsync<StorageData>("get", (object?)null);

    public async Task SetAsync(StorageData items) => await InvokeVoidAsync("set", items);

    public async Task SetAsync(string key, object? value) => await InvokeVoidAsync("set", new StorageData { { key, value } });

    public async Task RemoveAsync(string key) => await InvokeVoidAsync("remove", key);

    public async Task RemoveAsync(params string[] keys) => await InvokeVoidAsync("remove", keys);

    public async Task ClearAsync() => await InvokeVoidAsync("clear");

    public async Task<int> GetBytesInUseAsync(string? key = null) => key == null ? await InvokeAsync<int>("getBytesInUse") : await InvokeAsync<int>("getBytesInUse", key);

    public async Task<int> GetBytesInUseAsync(params string[] keys) => await InvokeAsync<int>("getBytesInUse", keys);
}