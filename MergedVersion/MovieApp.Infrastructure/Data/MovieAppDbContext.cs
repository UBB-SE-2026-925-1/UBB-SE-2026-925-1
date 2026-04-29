using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace MovieApp.Infrastructure;

/// <summary>
/// The primary database context for the MovieApp application, 
/// integrating entities from both legacy repositories into a unified schema.
/// </summary>
public sealed class MovieAppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovieAppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by this context.</param>
    public MovieAppDbContext(DbContextOptions<MovieAppDbContext> options)
        : base(options)
    {
    }

    // --- Core Catalog Sets ---
    public DbSet<Movie> Movies => this.Set<Movie>();
    public DbSet<Genre> Genres => this.Set<Genre>();
    public DbSet<Actor> Actors => this.Set<Actor>();
    public DbSet<Director> Directors => this.Set<Director>();

    // --- Social & Feedback Sets ---
    public DbSet<User> Users => this.Set<User>();
    public DbSet<Review> Reviews => this.Set<Review>();
    public DbSet<Comment> Comments => this.Set<Comment>();

    // --- Events & Ticketing Sets ---
    public DbSet<Event> Events => this.Set<Event>();
    public DbSet<Screening> Screenings => this.Set<Screening>();
    public DbSet<UserEventAttendance> UserEventAttendances => this.Set<UserEventAttendance>();
    public DbSet<FavoriteEvent> FavoriteEvents => this.Set<FavoriteEvent>();
    public DbSet<WatchedEvent> WatchedEvents => this.Set<WatchedEvent>();

    // --- Gamification & Rewards Sets ---
    public DbSet<Battle> Battles => this.Set<Battle>();
    public DbSet<Bet> Bets => this.Set<Bet>();
    public DbSet<Badge> Badges => this.Set<Badge>();
    public DbSet<UserBadge> UserBadges => this.Set<UserBadge>();
    public DbSet<UserStats> UserStats => this.Set<UserStats>();
    public DbSet<UserSpinData> UserSpinStates => this.Set<UserSpinData>();
    public DbSet<Reward> Rewards => this.Set<Reward>();

    // --- Marathon & Progression Sets ---
    public DbSet<Marathon> Marathons => this.Set<Marathon>();
    public DbSet<MarathonProgress> MarathonProgressions => this.Set<MarathonProgress>();
    public DbSet<TriviaQuestion> TriviaQuestions => this.Set<TriviaQuestion>();
    public DbSet<TriviaReward> TriviaRewards => this.Set<TriviaReward>();

    // --- Communication & Growth Sets ---
    public DbSet<Notification> Notifications => this.Set<Notification>();
    public DbSet<AmbassadorProfile> AmbassadorProfiles => this.Set<AmbassadorProfile>();
    public DbSet<ReferralLog> ReferralLogs => this.Set<ReferralLog>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // This line scans the current assembly for all classes implementing IEntityTypeConfiguration<T>
        // and applies the rules defined in your "Configurations" folder.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieAppDbContext).Assembly);
    }
}
