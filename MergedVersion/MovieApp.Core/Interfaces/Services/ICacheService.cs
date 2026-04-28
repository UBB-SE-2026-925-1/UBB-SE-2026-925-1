using System.Threading;
using System.Threading.Tasks;
#nullable enable

namespace MovieApp.Core.Interfaces.Service;

/// <summary>
/// Defines a contract for caching external network resources to improve performance.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a resource from the cache or fetches it from the network if missing.
    /// </summary>
    /// <param name="cacheKey">The unique key for the cached item.</param>
    /// <param name="url">The source URL to fetch from if the cache is empty.</param>
    /// <param name="client">The HTTP client used for the network request.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>The string content of the resource.</returns>
    Task<string> FetchOrCacheAsync(string cacheKey, string url, HttpClient client, CancellationToken ct = default);
}

