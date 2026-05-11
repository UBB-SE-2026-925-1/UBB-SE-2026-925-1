// <copyright file="CurrentUserService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.DTOs;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Resolves and caches the bootstrap user used by the application shell.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IUserRepository userRepository;
    private readonly BootstrapUserOptions bootstrapUserOptions;

    private CurrentUserDTO? currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    public CurrentUserService(
        IUserRepository userRepository,
        BootstrapUserOptions bootstrapUserOptions)
    {
        this.userRepository = userRepository;
        this.bootstrapUserOptions = bootstrapUserOptions;
    }

    /// <summary>
    /// Gets the initialized current user.
    /// </summary>
    public CurrentUserDTO CurrentUser =>
        this.currentUser
        ?? throw new InvalidOperationException(
            "The current user has not been initialized.");

    /// <summary>
    /// Loads the configured bootstrap user once and caches the result.
    /// </summary>
    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (this.currentUser is not null)
        {
            return;
        }

        var user = await this.userRepository.FindByAuthIdentityAsync(
            this.bootstrapUserOptions.AuthProvider,
            this.bootstrapUserOptions.AuthSubject,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"The configured bootstrap user '{this.bootstrapUserOptions.AuthProvider}:{this.bootstrapUserOptions.AuthSubject}' could not be found.");
        }

        this.currentUser = new CurrentUserDTO
        {
            Id = user.Id,
            Username = user.Username,
            TotalPoints = user.UserStats?.TotalPoints ?? 0,
            WeeklyScore = user.UserStats?.WeeklyScore ?? 0
        };
    }
}