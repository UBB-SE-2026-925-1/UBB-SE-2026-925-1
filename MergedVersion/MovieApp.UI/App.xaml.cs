using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;
using MovieApp.Infrastructure.Repositories;
using MovieApp.Proxy;
using MovieApp.UI.Services;
using MovieApp.UI.ViewModels;
using MovieApp.UI.ViewModels.Events;
using System.Net.Http;
using System.Net.Http.Json;

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
        System.Diagnostics.Debug.WriteLine(">>> App Starting...");

        // ── 1. Database seed (non-fatal: app opens even if seeding partially fails) ──
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MovieAppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await DbInitializer.SeedAsync(db);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> DB SEED WARNING: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($">>> INNER: {ex.InnerException.Message}");
            // Non-fatal: continue startup so the window still opens.
        }

        // ── 2. Auto-login: acquire JWT and inject into the singleton ApiClient ────
        // The UI app has no middleware pipeline, so we do this once here before
        // anything touches the API. Every remote service shares the same singleton
        // ApiClient, so one SetBearerToken call covers the whole app.
        try
        {
            System.Diagnostics.Debug.WriteLine(">>> Auto-login: acquiring JWT...");
            var baseUrl = Configuration["WebApi:BaseUrl"] ?? "http://localhost:5207";
            var username = Configuration["Authentication:BootstrapUser:Username"] ?? "Admin";
            var password = Configuration["Authentication:BootstrapUser:Password"] ?? "Admin123!";

            using var http = new HttpClient();
            var loginResponse = await http.PostAsJsonAsync(
                $"{baseUrl}/api/auth/login",
                new { Username = username, Password = password });

            if (loginResponse.IsSuccessStatusCode)
            {
                var result = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
                if (result?.Token is not null)
                {
                    var apiClient = ServiceProvider.GetRequiredService<ApiClient>();
                    apiClient.SetBearerToken(result.Token);
                    System.Diagnostics.Debug.WriteLine(">>> JWT injected into ApiClient.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($">>> Auto-login failed: {loginResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> Auto-login WARNING: {ex.Message}");
        }

        // ── 3. Current user + slot machine streak (non-fatal) ──
        try
        {
            System.Diagnostics.Debug.WriteLine(">>> Initializing CurrentUser via API...");
            var currentUserService = ServiceProvider.GetRequiredService<ICurrentUserService>();
            await currentUserService.InitializeAsync();
            CurrentUserId = currentUserService.CurrentUser.Id;
            MovieApp.Proxy.IdentityConfig.CurrentUserId = CurrentUserId;
            System.Diagnostics.Debug.WriteLine($">>> User '{currentUserService.CurrentUser.Username}' (ID: {CurrentUserId}) loaded.");

            System.Diagnostics.Debug.WriteLine(">>> Checking slot machine login streaks...");
            var slotMachineService = ServiceProvider.GetRequiredService<ISlotMachineService>();
            await slotMachineService.RecordLoginAndCheckStreakAsync(CurrentUserId);
            StreakSpinGrantedOnLogin = await slotMachineService.GrantStreakSpinAsync(CurrentUserId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> API INIT WARNING: {ex.Message}");
            // Non-fatal: app opens without a current user — pages that need the user
            // will surface individual errors rather than blocking the window.
        }

        // ── 4. Open the window unconditionally ──
        try
        {
            System.Diagnostics.Debug.WriteLine(">>> Launching MainWindow...");
            _window = ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
            System.Diagnostics.Debug.WriteLine(">>> App Started Successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> CRITICAL: Could not create MainWindow: {ex.Message}");
            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }
    }
    private record LoginResult(string Token);
}
