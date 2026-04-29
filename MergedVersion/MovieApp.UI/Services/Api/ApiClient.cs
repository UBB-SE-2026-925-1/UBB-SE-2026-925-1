using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.UI.Services.Api;

/// <summary>
/// Provides a centralized client for executing HTTP requests against the Web API.
/// </summary>
public class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        // Use camelCase to match the ASP.NET Core default
        this.jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        int retryCount = 0;
        const int maxRetries = 10;
        while (true)
        {
            try
            {
                using var response = await this.httpClient.GetAsync(endpoint, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {errorContent}");
                }

                var contentString = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(contentString))
                {
                    return default;
                }

                if (typeof(T) == typeof(string))
                {
                    if (contentString.StartsWith("\"") && contentString.EndsWith("\""))
                    {
                        return (T)(object)contentString.Trim('"');
                    }
                    return (T)(object)contentString;
                }

                return JsonSerializer.Deserialize<T>(contentString, this.jsonOptions);
            }
            catch (JsonException)
            {
                // If it's explicitly a JSON parsing error on a 200 OK, don't retry, just return default or throw.
                // We return default assuming the endpoint had nothing to return.
                return default;
            }
            catch (Exception) when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(2000, ct);
            }
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        var response = await this.httpClient.PostAsJsonAsync(endpoint, request, this.jsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {errorContent}");
        }
        return await response.Content.ReadFromJsonAsync<TResponse>(this.jsonOptions, ct);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        var response = await this.httpClient.PostAsJsonAsync(endpoint, request, this.jsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {errorContent}");
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var response = await this.httpClient.DeleteAsync(endpoint, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"API Error ({response.StatusCode}) on {endpoint}: {errorContent}");
        }
    }
}
