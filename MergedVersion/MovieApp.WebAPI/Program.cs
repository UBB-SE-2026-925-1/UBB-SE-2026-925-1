using Microsoft.EntityFrameworkCore;
using MovieApp.Infrastructure;
using MovieApp.Core.Repositories;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Infrastructure.Repositories;
using MovieApp.Infrastructure.Data;
using MovieApp.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// 0. Add Options
builder.Services.AddSingleton(new BootstrapUserOptions
{
    AuthProvider = "Seed",
    AuthSubject = "Admin"
});

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
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IReferralCodeGenerator, ReferralCodeGenerator>();
builder.Services.AddScoped<IReferralLogService, ReferralLogService>();
builder.Services.AddScoped<IReferralValidator, ReferralValidator>();
builder.Services.AddScoped<IRewardService, RewardService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});



var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MovieAppDbContext>();
        context.Database.Migrate();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
