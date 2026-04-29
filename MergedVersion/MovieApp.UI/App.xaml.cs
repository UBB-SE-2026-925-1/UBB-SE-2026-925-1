using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Services;
using MovieApp.UI.ViewModels;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Service;
using MovieApp.UI.Services;
using MovieApp.UI.Services.Api;
using MovieApp.UI.ViewModels.Events;
using System.Net.Http;

namespace MovieApp.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public Window? _window;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static IConfiguration Configuration { get; private set; } = null!;
    public static IAppServices Services => ServiceProvider.GetRequiredService<IAppServices>();
    public static int CurrentUserId { get; private set; } = 1; // Default for testing
    public static bool StreakSpinGrantedOnLogin { get; private set; }
    public static MainWindow CurrentMainWindow => (MainWindow)((App)Application.Current)._window!;
    public static bool EnsureServicesValid() => ServiceProvider != null;

    public App()
    {
        InitializeComponent();
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        Configuration = builder.Build();
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. API Client Configuration
        services.AddSingleton<HttpClient>(new HttpClient { BaseAddress = new Uri("http://localhost:5207/") });
        services.AddSingleton<ApiClient>();

        // 2. Remote Repositories (only where needed by legacy views)
        services.AddScoped<IEventRepository, RemoteEventRepository>();
        services.AddScoped<ITriviaRepository, RemoteTriviaRepository>();
        services.AddScoped<ITriviaRewardRepository, RemoteTriviaRewardRepository>();
        services.AddScoped<IUserSlotMachineStateRepository, RemoteUserSlotMachineStateRepository>();
        services.AddScoped<IAmbassadorRepository, RemoteAmbassadorRepository>();
        services.AddScoped<IScreeningRepository, RemoteScreeningRepository>();
        services.AddScoped<IUserMovieDiscountRepository, RemoteUserMovieDiscountRepository>();
        services.AddScoped<IUserEventAttendanceRepository, RemoteUserEventAttendanceRepository>();
        services.AddScoped<IPriceWatcherRepository, RemotePriceWatcherRepository>();

        // 3. Remote Services
        services.AddScoped<ICatalogService, RemoteCatalogService>();
        services.AddScoped<IReviewService, RemoteReviewService>();
        services.AddScoped<ICommentService, RemoteCommentService>();
        services.AddScoped<IBattleService, RemoteBattleService>();
        services.AddScoped<IBadgeService, RemoteBadgeService>();
        services.AddSingleton<IPointService, RemotePointService>();
        services.AddSingleton<IMarathonService, RemoteMarathonService>();
        services.AddSingleton<INotificationService, RemoteNotificationService>();
        services.AddSingleton<ISlotMachineService, RemoteSlotMachineService>();
        services.AddSingleton<IFavoriteEventService, RemoteFavoriteEventService>();
        services.AddSingleton<ICurrentUserService, RemoteCurrentUserService>();
        
        // These can stay local as they are logic-only or not yet API-ified
        services.AddSingleton<ISlotMachineResultService, SlotMachineResultService>();
        services.AddSingleton<IReferralCodeGenerator, ReferralCodeGenerator>();
        services.AddSingleton<IReferralLogService, ReferralLogService>();
        services.AddSingleton<IReferralValidator, ReferralValidator>();
        services.AddSingleton<IRewardService, RewardService>();
        services.AddSingleton<ExternalReviewService>();

        // 3.5 Options
        services.Configure<BootstrapUserOptions>(Configuration.GetSection("Authentication:BootstrapUser"));
        services.AddSingleton(resolver => 
            resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<BootstrapUserOptions>>().Value);
        
        // Legacy Support
        services.AddSingleton<IAppServices, DIAppServices>();
        services.AddSingleton<ReelAnimationService>();
        services.AddSingleton<ISlotMachineAnimationService, SlotMachineAnimationService>();
        services.AddSingleton<SlotMachineAnimationService>();
        services.AddSingleton<IWatchlistPathProvider, WatchlistPathProvider>();
        services.AddSingleton<IDialogService, WinUiDialogService>();
        services.AddSingleton<IEventUserStateService, EventUserStateService>();
        services.AddSingleton<IEventJoinService, EventJoinService>();

        // 4. ViewModels
        services.AddTransient<CatalogViewModel>();
        services.AddTransient<BattleViewModel>();
        services.AddTransient<ForumViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<MovieDetailViewModel>();
        services.AddTransient<MovieApp.UI.ViewModels.Events.HomeEventsViewModel>();
        services.AddTransient<FavoritesViewModel>();
        services.AddTransient<MarathonPageViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<RewardsViewModel>(provider => 
        {
            var currentUserService = provider.GetRequiredService<ICurrentUserService>();
            return new RewardsViewModel(
                provider.GetRequiredService<ITriviaRewardRepository>(),
                currentUserService.CurrentUser.Id);
        });
        services.AddTransient<SlotMachineViewModel>(provider => 
        {
            var currentUserService = provider.GetRequiredService<ICurrentUserService>();
            return new SlotMachineViewModel(
                currentUserService.CurrentUser.Id,
                provider.GetRequiredService<ISlotMachineService>(),
                provider.GetRequiredService<ISlotMachineAnimationService>());
        });
        services.AddTransient<MyEventsViewModel>();
        services.AddTransient<TriviaWheelViewModel>(provider => 
        {
            var currentUserService = provider.GetRequiredService<ICurrentUserService>();
            return new TriviaWheelViewModel(
                provider.GetRequiredService<ITriviaRepository>(),
                provider.GetRequiredService<ITriviaRewardRepository>(),
                provider.GetRequiredService<IUserSlotMachineStateRepository>(),
                currentUserService.CurrentUser.Id);
        });
        services.AddTransient<JackpotDialogViewModel>();

        // 5. Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine(">>> App Starting...");
            
            System.Diagnostics.Debug.WriteLine(">>> Step 1: Initializing CurrentUser via API...");
            var currentUserService = ServiceProvider.GetRequiredService<ICurrentUserService>();
            await currentUserService.InitializeAsync();
            CurrentUserId = currentUserService.CurrentUser.Id;
            System.Diagnostics.Debug.WriteLine($">>> Step 1: User {currentUserService.CurrentUser.Username} (ID: {CurrentUserId}) loaded.");

            System.Diagnostics.Debug.WriteLine(">>> Step 2: Launching MainWindow...");
            _window = ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
            System.Diagnostics.Debug.WriteLine(">>> App Started Successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> CRITICAL STARTUP ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($">>> INNER ERROR: {ex.InnerException.Message}");
            }
            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }
    }
}
