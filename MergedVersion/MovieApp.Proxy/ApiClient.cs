using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace MovieApp.Proxy;

/// <summary>
/// Centralized HTTP client for executing requests against the Web API.
/// Attach a Bearer token via SetBearerToken() once Person 2's JWT flow is in place.
/// </summary>
public class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Attaches a JWT Bearer token to every subsequent request from this instance.
    /// Called by Person 2's authentication middleware after login.
    /// </summary>
    public void SetBearerToken(string token)
    {
        this.httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        int retryCount = 0;
        const int maxRetries = 2;
        while (true)
        {
            try
            {
                using var response = await this.httpClient.GetAsync(endpoint, ct);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return default;
                    }

                    if (IsTransientStatusCode(response.StatusCode) && retryCount < maxRetries)
                    {
                        retryCount++;
                        await Task.Delay(250 * retryCount, ct);
                        continue;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(content)) return default;

                if (typeof(T) == typeof(string))
                {
                    if (content.StartsWith("\"") && content.EndsWith("\""))
                        return (T)(object)content.Trim('"');
                    return (T)(object)content;
                }

                return JsonSerializer.Deserialize<T>(content, this.jsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException) when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(250 * retryCount, ct);
            }
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        var response = await this.httpClient.PostAsJsonAsync(endpoint, request, this.jsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {error}");
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(this.jsonOptions, ct);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        var response = await this.httpClient.PostAsJsonAsync(endpoint, request, this.jsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {error}");
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var response = await this.httpClient.DeleteAsync(endpoint, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {error}");
        }
    }
}