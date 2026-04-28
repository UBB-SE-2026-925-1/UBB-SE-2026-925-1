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
                return await this.httpClient.GetFromJsonAsync<T>(endpoint, this.jsonOptions, ct);
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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(this.jsonOptions, ct);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken ct = default)
    {
        var response = await this.httpClient.PostAsJsonAsync(endpoint, request, this.jsonOptions, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var response = await this.httpClient.DeleteAsync(endpoint, ct);
        response.EnsureSuccessStatusCode();
    }
}
