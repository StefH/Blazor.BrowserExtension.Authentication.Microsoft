using System.Text.Json;
using StorageData = System.Collections.Generic.Dictionary<string, object?>;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public interface IChromeStorageLocal : IDisposable, IAsyncDisposable
{
    StorageData? Get(params string[] keys);

    StorageData? Get(StorageData keys);

    JsonElement? GetSingle(string key);

    string? GetSingleString(string key);

    StorageData? GetAll();

    void Set(StorageData items);

    void Set(string key, object? value);

    void Remove(string[] keys);

    void Clear();

    int GetBytesInUse(string? key = null);

    int GetBytesInUse(params string[] keys);

    ValueTask<StorageData?> GetAsync(params string[] keys);

    ValueTask<StorageData?> GetAsync(StorageData keys);

    ValueTask<JsonElement?> GetSingleAsync(string key);

    ValueTask<string?> GetSingleStringAsync(string key);

    ValueTask<StorageData?> GetAllAsync();

    ValueTask SetAsync(StorageData items);

    ValueTask SetAsync(string key, object? value);
    
    ValueTask RemoveAsync(params string[] keys);

    ValueTask ClearAsync();

    ValueTask<int> GetBytesInUseAsync(string? key = null);

    ValueTask<int> GetBytesInUseAsync(params string[] keys);
}