using MovieApp.Core.Services;
using MovieApp.Proxy;

namespace MovieApp.Web.Services;

public class ApiClientTokenInitializer
{
    private readonly RequestDelegate _next;

    public ApiClientTokenInitializer(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApiClient apiClient, JwtTokenStore tokenStore, ICurrentUserService currentUserService)
    {
        if (tokenStore.HasToken)
        {
            apiClient.SetBearerToken(tokenStore.Token!);

            // Token is now on the ApiClient - safe to initialize the user
            await currentUserService.InitializeAsync();
        }

        await _next(context);
    }
}