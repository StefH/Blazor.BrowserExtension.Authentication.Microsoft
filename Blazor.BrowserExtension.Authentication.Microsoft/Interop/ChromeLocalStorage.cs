using System.Text.Json;
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

    public void Remove(params string[] keys) => InvokeVoid("remove", keys, null);

    public void Clear() => InvokeVoid("clear");

    public int GetBytesInUse(string? key = null) => key == null ? Invoke<int>("getBytesInUse") : Invoke<int>("getBytesInUse", key);

    public int GetBytesInUse(params string[] keys) => Invoke<int>("getBytesInUse", keys);

    public async ValueTask<JsonElement?> GetSingleAsync(string key)
    {
        var result = await GetAsync(key);
        return result != null && result.TryGetValue(key, out var value) && value is JsonElement element ? element : null;
    }

    public async ValueTask<string?> GetSingleStringAsync(string key)
    {
        var result = await GetSingleAsync(key);
        return result?.GetString();
    }

    public ValueTask<StorageData?> GetAsync(params string[] keys) => InvokeAsync<StorageData>("get", keys);

    public ValueTask<StorageData?> GetAsync(StorageData keys) => InvokeAsync<StorageData>("get", keys);

    public ValueTask<StorageData?> GetAllAsync() => InvokeAsync<StorageData>("get", (object?)null);

    public ValueTask SetAsync(StorageData items) => InvokeVoidAsync("set", items);

    public ValueTask SetAsync(string key, object? value) => InvokeVoidAsync("set", new StorageData { { key, value } });

    public ValueTask RemoveAsync(params string[] keys) => InvokeVoidAsync("remove", keys, null);

    public ValueTask ClearAsync() => InvokeVoidAsync("clear");

    public ValueTask<int> GetBytesInUseAsync(string? key = null) => key == null ? InvokeAsync<int>("getBytesInUse") : InvokeAsync<int>("getBytesInUse", key);

    public ValueTask<int> GetBytesInUseAsync(params string[] keys) => InvokeAsync<int>("getBytesInUse", keys);
}