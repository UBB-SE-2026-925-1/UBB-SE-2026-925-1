using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Infrastructure.Data;
using MovieApp.Infrastructure.Repositories;
using MovieApp.WebAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DB Context
builder.Services.AddDbContext<MovieAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBattleRepository, BattleRepository>();
builder.Services.AddScoped<IBetRepository, BetRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
builder.Services.AddScoped<IUserStatsRepository, UserStatsRepository>();
builder.Services.AddScoped<IAmbassadorRepository, AmbassadorRepository>();
builder.Services.AddScoped<IMarathonRepository, MarathonRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IScreeningRepository, ScreeningRepository>();
builder.Services.AddScoped<ITriviaRepository, TriviaRepository>();
builder.Services.AddScoped<ITriviaRewardRepository, TriviaRewardRepository>();
builder.Services.AddScoped<IFavoriteEventRepository, FavoriteEventRepository>();
builder.Services.AddScoped<IUserEventAttendanceRepository, UserEventAttendanceRepository>();
builder.Services.AddScoped<IUserSlotMachineStateRepository, UserSlotMachineStateRepository>();
builder.Services.AddScoped<IPriceWatcherRepository, PriceWatcherRepository>();
builder.Services.AddScoped<IUserMovieDiscountRepository, UserMovieDiscountRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// 3. Add Services
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IBattleService, BattleService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<IMarathonService, MarathonService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISlotMachineService, SlotMachineService>();
builder.Services.AddScoped<ISlotMachineResultService, SlotMachineResultService>();
builder.Services.AddScoped<IFavoriteEventService, FavoriteEventService>();
builder.Services.AddScoped<IReferralCodeGenerator, ReferralCodeGenerator>();
builder.Services.AddScoped<IReferralLogService, ReferralLogService>();
builder.Services.AddScoped<IReferralValidator, ReferralValidator>();
builder.Services.AddScoped<IRewardService, RewardService>();

// 4. CurrentUserService — WebAPI implementation reads from JWT ClaimsPrincipal
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

// 5. JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

//builder.WebHost.UseUrls("http://localhost:5207");

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MovieAppDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ORDER MATTERS: Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var currentUserService = context.RequestServices
            .GetRequiredService<ICurrentUserService>();
        await currentUserService.InitializeAsync();
    }
    await next();
});

app.MapControllers();
app.Run();