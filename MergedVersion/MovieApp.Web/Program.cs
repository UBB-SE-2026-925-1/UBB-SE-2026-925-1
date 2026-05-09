using MovieApp.Core.Interfaces;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── HTTP client ──────────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["WebApi:BaseUrl"] ?? "http://localhost:5207";

builder.Services.AddHttpClient("MovieApi", client =>
    client.BaseAddress = new Uri(apiBaseUrl));

// ApiClient is scoped so each request gets its own instance (important for
// per-request Bearer token injection once Person 2's JWT flow is wired in).
builder.Services.AddScoped<ApiClient>(sp =>
    new ApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("MovieApi")));

// ── Remote services (all call the WebAPI via ApiClient) ──────────────────────
builder.Services.AddScoped<ICatalogService, RemoteCatalogService>();
builder.Services.AddScoped<IReviewService, RemoteReviewService>();
builder.Services.AddScoped<ICommentService, RemoteCommentService>();
builder.Services.AddScoped<ICurrentUserService, RemoteCurrentUserService>();

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
