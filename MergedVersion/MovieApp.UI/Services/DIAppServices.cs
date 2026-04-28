using Microsoft.Extensions.DependencyInjection;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.UI.Services;
using System;

namespace MovieApp.UI.Services;

public class DIAppServices : IAppServices
{
    private readonly IServiceProvider _serviceProvider;

    public DIAppServices(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICurrentUserService? CurrentUserService => _serviceProvider.GetService<ICurrentUserService>();
    public IPriceWatcherRepository? PriceWatcherRepository => _serviceProvider.GetService<IPriceWatcherRepository>();
    public IEventRepository? EventRepository => _serviceProvider.GetService<IEventRepository>();
    public ITriviaRepository? TriviaRepository => _serviceProvider.GetService<ITriviaRepository>();
    public ITriviaRewardRepository? TriviaRewardRepository => _serviceProvider.GetService<ITriviaRewardRepository>();
    public IAmbassadorRepository? AmbassadorRepository => _serviceProvider.GetService<IAmbassadorRepository>();
    public IReferralValidator? ReferralValidator => _serviceProvider.GetService<IReferralValidator>();
    public IMarathonRepository? MarathonRepository => _serviceProvider.GetService<IMarathonRepository>();
    public IFavoriteEventService? FavoriteEventService => _serviceProvider.GetService<IFavoriteEventService>();
    public INotificationService? NotificationService => _serviceProvider.GetService<INotificationService>();
    public IMovieRepository? MovieRepository => _serviceProvider.GetService<IMovieRepository>();
    public IUserSlotMachineStateRepository? SlotMachineStateRepository => _serviceProvider.GetService<IUserSlotMachineStateRepository>();
    public IUserMovieDiscountRepository? UserMovieDiscountRepository => _serviceProvider.GetService<IUserMovieDiscountRepository>();
    public IScreeningRepository? ScreeningRepository => _serviceProvider.GetService<IScreeningRepository>();
    public IUserEventAttendanceRepository? UserEventAttendanceRepository => _serviceProvider.GetService<IUserEventAttendanceRepository>();
    public ISlotMachineService? SlotMachineService => _serviceProvider.GetService<ISlotMachineService>();
    public ISlotMachineResultService? SlotMachineResultService => _serviceProvider.GetService<ISlotMachineResultService>();
    public ReelAnimationService? ReelAnimationService => _serviceProvider.GetService<ReelAnimationService>();
    public SlotMachineAnimationService? SlotMachineAnimationService => _serviceProvider.GetService<SlotMachineAnimationService>();
    public IEventUserStateService? EventUserStateService => _serviceProvider.GetService<IEventUserStateService>();
    public IEventJoinService? EventJoinService => _serviceProvider.GetService<IEventJoinService>();
    public IWatchlistPathProvider? WatchlistPathProvider => _serviceProvider.GetService<IWatchlistPathProvider>();
    public IMarathonService? MarathonService => _serviceProvider.GetService<IMarathonService>();
    public IDialogService? DialogService => _serviceProvider.GetService<IDialogService>();
}
