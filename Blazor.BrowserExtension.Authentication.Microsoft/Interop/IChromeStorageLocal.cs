using System;
using System.Text.Json;
using System.Threading.Tasks;
using StorageData = System.Collections.Generic.Dictionary<string, object?>;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Interop;

public interface IChromeStorageLocal : IDisposable, IAsyncDisposable
{
    StorageData? Get(string key);

    StorageData? Get(params string[] keys);

    StorageData? Get(StorageData keys);

    JsonElement? GetSingle(string key);

    string? GetSingleString(string key);

    StorageData? GetAll();

    void Set(StorageData items);

    void Set(string key, object? value);

    void Remove(string key);

    void Remove(string[] keys);

    void Clear();

    int GetBytesInUse(string? key = null);

    int GetBytesInUse(params string[] keys);

    Task<StorageData?> GetAsync(string key);

    Task<StorageData?> GetAsync(params string[] keys);

    Task<StorageData?> GetAsync(StorageData keys);

    Task<JsonElement?> GetSingleAsync(string key);

    Task<string?> GetSingleStringAsync(string key);

    Task<StorageData?> GetAllAsync();

    Task SetAsync(StorageData items);

    Task SetAsync(string key, object? value);

    Task RemoveAsync(string key);

    Task RemoveAsync(params string[] keys);

    Task ClearAsync();

    Task<int> GetBytesInUseAsync(string? key = null);

    Task<int> GetBytesInUseAsync(params string[] keys);
}