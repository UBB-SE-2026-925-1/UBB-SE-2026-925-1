using MovieApp.Proxy;

namespace MovieApp.Web.Services;

public class AutoLoginService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutoLoginService> _logger;

    public AutoLoginService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<AutoLoginService> logger)
    {
        this._serviceProvider = serviceProvider;
        this._configuration = configuration;
        this._logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Small delay to ensure the API is up (especially if running both projects together)

        await Task.Delay(500, cancellationToken);

        var username = _configuration["BootstrapUser:Username"];
        var password = _configuration["BootstrapUser:Password"]!;
        var baseUrl = _configuration["WebApi:BaseUrl"]!;

        try
        {
            using var http = new HttpClient();
            var response = await http.PostAsJsonAsync(
                $"{baseUrl}/api/auth/login",
                new { Username = username, Password = password },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Auto-login failed: {Status}", response.StatusCode);
                return;
            }

            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

            if (result?.Token is null)
            {
                _logger.LogError("auto-login succeeded but token was null.");
                return;
            }

            //Store the token
            using var scope = _serviceProvider.CreateScope();
            var tokenStore = scope.ServiceProvider.GetRequiredService<JwtTokenStore>();
            tokenStore.SetToken(result.Token);

            //Inject into the shared ApiClient
            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
            apiClient.SetBearerToken(result.Token);

            _logger.LogInformation("Auto-login successful. JWT acquired and injected.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during auto-login");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private record LoginResponse(string Token);
}