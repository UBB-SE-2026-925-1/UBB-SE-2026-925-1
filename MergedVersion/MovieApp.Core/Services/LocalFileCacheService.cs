using System.Threading;
using System.Threading.Tasks;
#nullable enable

using MovieApp.Core.Interfaces.Service;

namespace MovieApp.Core.Services;

/// <summary>
/// Implements a simple file-based cache for API responses.
/// </summary>
public sealed class LocalFileCacheService : ICacheService
{
    private readonly string cacheDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileCacheService"/> class.
    /// </summary>
    public LocalFileCacheService()
    {
        this.cacheDirectory = Path.Combine(AppContext.BaseDirectory, "ApiCache");
        Directory.CreateDirectory(this.cacheDirectory);
    }

    /// <inheritdoc/>
    public async Task<string> FetchOrCacheAsync(string cacheKey, string url, HttpClient client, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(client);

        var cachePath = Path.Combine(this.cacheDirectory, $"{cacheKey}.json");

        if (File.Exists(cachePath))
        {
            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath);
            if (age < TimeSpan.FromHours(24))
            {
                return await File.ReadAllTextAsync(cachePath, ct);
            }
        }

        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        await File.WriteAllTextAsync(cachePath, json, ct);
        return json;
    }
}

