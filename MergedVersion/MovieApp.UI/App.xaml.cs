using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Services;
using MovieApp.UI.ViewModels;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Service;
using MovieApp.UI.Services;
using MovieApp.Proxy;
//using MovieApp.UI.Services.Api;
using MovieApp.UI.ViewModels.Events;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;
using MovieApp.Infrastructure.Repositories;

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
    public static int CurrentUserId { get; private set; } = 1;
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
        // 1. Database
        services.AddDbContextFactory<MovieAppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // 2. API Client
        services.AddSingleton<HttpClient>(new HttpClient { BaseAddress = new Uri("http://localhost:5207/") });
        services.AddSingleton<ApiClient>();

        // 3. Repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITriviaRepository, RemoteTriviaRepository>();
        services.AddScoped<ITriviaRewardRepository, RemoteTriviaRewardRepository>();
        services.AddScoped<IUserSlotMachineStateRepository, RemoteUserSlotMachineStateRepository>();
        services.AddScoped<IAmbassadorRepository, RemoteAmbassadorRepository>();
        services.AddScoped<IScreeningRepository, RemoteScreeningRepository>();
        services.AddScoped<IUserMovieDiscountRepository, RemoteUserMovieDiscountRepository>();
        services.AddTransient<IUserEventAttendanceRepository, UserEventAttendanceRepository>();
        services.AddScoped<IPriceWatcherRepository, RemotePriceWatcherRepository>();
        services.AddScoped<IBookingRepository, RemoteBookingRepository>();

        // 4. Services
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

        services.AddSingleton<ISlotMachineResultService, SlotMachineResultService>();
        services.AddSingleton<IReferralCodeGenerator, ReferralCodeGenerator>();
        services.AddSingleton<IReferralLogService, ReferralLogService>();
        services.AddSingleton<IReferralValidator, ReferralValidator>();
        services.AddSingleton<IRewardService, RewardService>();
        services.AddSingleton<ExternalReviewService>();

        services.Configure<BootstrapUserOptions>(Configuration.GetSection("Authentication:BootstrapUser"));
        services.AddSingleton(resolver =>
            resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<BootstrapUserOptions>>().Value);

        services.AddSingleton<IAppServices, DIAppServices>();
        services.AddSingleton<ReelAnimationService>();
        services.AddSingleton<ISlotMachineAnimationService, SlotMachineAnimationService>();
        services.AddSingleton<SlotMachineAnimationService>();
        services.AddSingleton<IWatchlistPathProvider, WatchlistPathProvider>();
        services.AddSingleton<IDialogService, WinUiDialogService>();
        services.AddSingleton<IEventUserStateService, EventUserStateService>();
        services.AddSingleton<IEventJoinService, EventJoinService>();

        // 5. ViewModels
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
                provider.GetRequiredService<ISlotMachineAnimationService>(),
                provider.GetRequiredService<IUserMovieDiscountRepository>());
        });
        services.AddTransient<MyEventsViewModel>(provider => new MyEventsViewModel(
                provider.GetRequiredService<IPriceWatcherRepository>(),
                provider.GetRequiredService<IEventRepository>(),
                provider.GetRequiredService<IUserEventAttendanceRepository>()
        ));
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

        // 6. Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine(">>> App Starting...");

            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MovieAppDbContext>();
                await db.Database.EnsureCreatedAsync();
                await DbInitializer.SeedAsync(db);
            }

            System.Diagnostics.Debug.WriteLine(">>> Initializing CurrentUser via API...");
            var currentUserService = ServiceProvider.GetRequiredService<ICurrentUserService>();
            await currentUserService.InitializeAsync();
            CurrentUserId = currentUserService.CurrentUser.Id;
            System.Diagnostics.Debug.WriteLine($">>> User '{currentUserService.CurrentUser.Username}' (ID: {CurrentUserId}) loaded.");

            System.Diagnostics.Debug.WriteLine(">>> Checking slot machine login streaks...");
            var slotMachineService = ServiceProvider.GetRequiredService<ISlotMachineService>();
            await slotMachineService.RecordLoginAndCheckStreakAsync(CurrentUserId);
            StreakSpinGrantedOnLogin = await slotMachineService.GrantStreakSpinAsync(CurrentUserId);

            System.Diagnostics.Debug.WriteLine(">>> Launching MainWindow...");
            _window = ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
            System.Diagnostics.Debug.WriteLine(">>> App Started Successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> CRITICAL STARTUP ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($">>> INNER ERROR: {ex.InnerException.Message}");
            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }
    }
}
