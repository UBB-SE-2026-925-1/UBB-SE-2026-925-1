using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// ── HTTP client ──────────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["WebApi:BaseUrl"] ?? "http://localhost:5207";

builder.Services.AddHttpClient("MovieApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10); // fail fast so pages show error banner instead of hanging
});

// ApiClient is scoped so each request gets its own instance (important for
// per-request Bearer token injection once Person 2's JWT flow is wired in).
builder.Services.AddScoped<ApiClient>(sp =>
    new ApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("MovieApi")));

// ── Remote services (all call the WebAPI via ApiClient) ──────────────────────
builder.Services.AddScoped<ICatalogService, RemoteCatalogService>();
builder.Services.AddScoped<IReviewService, RemoteReviewService>();
builder.Services.AddScoped<ICommentService, RemoteCommentService>();
builder.Services.AddScoped<ICurrentUserService, RemoteCurrentUserService>();
builder.Services.AddScoped<IScreeningRepository, RemoteScreeningRepository>();
builder.Services.AddScoped<IBookingRepository, RemoteBookingRepository>();
builder.Services.AddScoped<ISlotMachineService, RemoteSlotMachineService>();
builder.Services.AddScoped<ITriviaRepository, RemoteTriviaRepository>();
builder.Services.AddScoped<ITriviaRewardRepository, RemoteTriviaRewardRepository>();
builder.Services.AddScoped<IBattleService, RemoteBattleService>();
builder.Services.AddScoped<IPointService, RemotePointService>();


// ExternalReviewService is registered with no providers for now.
// ASP.NET Core DI resolves IEnumerable<IExternalReviewProvider> as empty when
// no providers are registered — GetExternalReviewsAsync returns [] gracefully.
builder.Services.AddSingleton<ExternalReviewService>();

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
