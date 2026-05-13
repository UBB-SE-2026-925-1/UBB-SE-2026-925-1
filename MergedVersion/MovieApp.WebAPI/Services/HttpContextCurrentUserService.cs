using MovieApp.Core.DTOs;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MovieApp.WebAPI.Services;

/// <summary>
/// Resolves the current user from the JWT ClaimsPrincipal in the HTTP context.
/// Replaces the bootstrap-based CurrentUserService for authenticated API requests
/// </summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IUserRepository userRepository;
    private CurrentUserDTO? currentUser;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userRepository = userRepository;
    }

    public CurrentUserDTO CurrentUser =>
        this.currentUser
        ?? throw new InvalidOperationException(
            "Current user has not been initialized. Was InitializeAsync called?");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.currentUser is not null)
        {
            return;
        }

        var user = this.httpContextAccessor.HttpContext?.User;

        var subClaim = user?.FindFirst(JwtRegisteredClaimNames.Sub)
                       ?? user?.FindFirst(ClaimTypes.NameIdentifier);

        if (subClaim is null || !int.TryParse(subClaim.Value, out var userId))
        {
            throw new InvalidOperationException("JWT does not contain a valid 'sub' claim");
        }

        var dbUser = await this.userRepository.GetByIdAsync(userId, cancellationToken);

        if (dbUser is null)
        {
            throw new InvalidOperationException($"User with id {userId} not found.");
        }

        this.currentUser = new CurrentUserDTO
        {
            Id = dbUser.Id,
            Username = dbUser.Username,
            TotalPoints = dbUser.UserStats?.TotalPoints ?? 0,
            WeeklyScore = dbUser.UserStats?.WeeklyScore ?? 0
        };
    }
}