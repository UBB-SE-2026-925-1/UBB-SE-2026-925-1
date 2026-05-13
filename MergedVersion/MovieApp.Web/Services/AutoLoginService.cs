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
        var username = _configuration["BootstrapUser:Username"];
        var password = _configuration["BootstrapUser:Password"]!;
        var baseUrl = _configuration["WebApi:BaseUrl"]!;

        // The API may still be starting up (migrations + seeding on cold DB).
        // Retry with backoff until it answers, up to ~30s total.
        const int maxAttempts = 15;
        var delay = TimeSpan.FromMilliseconds(500);
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                var response = await http.PostAsJsonAsync(
                    $"{baseUrl}/api/auth/login",
                    new { Username = username, Password = password },
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

                    if (result?.Token is null)
                    {
                        _logger.LogError("Auto-login succeeded but token was null.");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var tokenStore = scope.ServiceProvider.GetRequiredService<JwtTokenStore>();
                    tokenStore.SetToken(result.Token);

                    _logger.LogInformation("Auto-login successful on attempt {Attempt}. JWT acquired.", attempt);
                    return;
                }

                _logger.LogWarning("Auto-login attempt {Attempt} returned {Status}.", attempt, response.StatusCode);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning("Auto-login attempt {Attempt} failed: {Message}. Retrying...", attempt, ex.Message);
            }

            await Task.Delay(delay, cancellationToken);
            if (delay < TimeSpan.FromSeconds(3))
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 1.5);
        }

        _logger.LogError("Auto-login gave up after {Attempts} attempts.", maxAttempts);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private record LoginResponse(string Token);
}