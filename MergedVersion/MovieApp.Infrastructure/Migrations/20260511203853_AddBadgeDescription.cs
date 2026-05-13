using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MovieApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Movies",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Badges",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SeatBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreeningId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatBookings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Actors",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Tom Cruise" },
                    { 2, "Scarlett Johansson" },
                    { 3, "Leonardo DiCaprio" },
                    { 4, "Emma Watson" },
                    { 5, "Dwayne Johnson" },
                    { 6, "Jennifer Lawrence" },
                    { 7, "Tom Hanks" },
                    { 8, "Angelina Jolie" },
                    { 9, "Chris Evans" },
                    { 10, "Natalie Portman" },
                    { 11, "Johnny Depp" },
                    { 12, "Meryl Streep" }
                });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "BadgeId", "CriteriaValue", "Description", "Name" },
                values: new object[,]
                {
                    { 1, 10, "Reach 10 profile progress points.", "Film Fanatic" },
                    { 2, 5, "Reach 5 profile progress points.", "Reviewer" },
                    { 3, 20, "Reach 20 profile progress points.", "Social Butterfly" },
                    { 4, 1, "Reach 1 profile progress point.", "Early Bird" },
                    { 5, 50, "Reach 50 profile progress points.", "Movie Buff" },
                    { 6, 100, "Reach 100 profile progress points.", "Elite Member" }
                });

            migrationBuilder.InsertData(
                table: "Directors",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Steven Spielberg" },
                    { 2, "Christopher Nolan" },
                    { 3, "Martin Scorsese" },
                    { 4, "Quentin Tarantino" },
                    { 5, "Ridley Scott" },
                    { 6, "James Cameron" },
                    { 7, "Spike Lee" },
                    { 8, "David Fincher" },
                    { 9, "Denis Villeneuve" },
                    { 10, "Wes Anderson" },
                    { 11, "Ari Aster" },
                    { 12, "Paul Thomas Anderson" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "CreatorUserId", "CurrentEnrollment", "Description", "DiscountPercentage", "EventDateTime", "EventType", "HistoricalRating", "IsJoined", "LocationReference", "MaxCapacity", "PosterUrl", "TicketPrice", "Title" },
                values: new object[,]
                {
                    { 1, 1, 45, "Special screening of the Palme d'Or winner.", 0, new DateTime(2026, 5, 15, 19, 0, 0, 0, DateTimeKind.Unspecified), "Premiere", 4.7999999999999998, false, "Cinema Hall A", 100, "", 25.00m, "Cannes Winner Screening" },
                    { 2, 1, 10, "Back-to-back classics from the 50s.", 0, new DateTime(2026, 5, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), "Marathon", 4.5, false, "Retro Cinema", 50, "", 40.00m, "Vintage Film Marathon" },
                    { 3, 1, 150, "Watch the latest sci-fi hit followed by a talk.", 0, new DateTime(2026, 5, 25, 20, 0, 0, 0, DateTimeKind.Unspecified), "Special", 4.9000000000000004, false, "Tech Hub Theater", 200, "", 15.00m, "Director's Q&A: Sci-Fi Night" }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Action" },
                    { 2, "Comedy" },
                    { 3, "Drama" },
                    { 4, "Horror" },
                    { 5, "Romance" },
                    { 6, "Science Fiction" },
                    { 7, "Thriller" },
                    { 8, "Animation" },
                    { 9, "Adventure" },
                    { 10, "Mystery" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "AverageRating", "Description", "DurationMinutes", "PosterUrl", "ReleaseYear", "Title" },
                values: new object[,]
                {
                    { 1, 3.7999999999999998, "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.", 0, "https://image.tmdb.org/t/p/w500/20f2GThu22hp5MgCA4dg3bZ3gTS.jpg", 1994, "The Shawshank Redemption" },
                    { 2, 0.0, "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.", 0, "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg", 2008, "The Dark Knight" },
                    { 3, 0.0, "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.", 0, "https://image.tmdb.org/t/p/w500/ljsZTbVsrQSqZgWeep2B1QiDKuh.jpg", 2010, "Inception" },
                    { 4, 0.0, "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.", 0, "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg", 1994, "Pulp Fiction" },
                    { 5, 5.0, "A paraplegic Marine dispatched to the moon Pandora on a unique mission becomes torn between following his orders and protecting the world he feels is his home.", 0, "https://image.tmdb.org/t/p/w500/kyeqWdyUXW608qlYkRqosgbbJyK.jpg", 2009, "Avatar" },
                    { 6, 0.0, "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", 0, "https://image.tmdb.org/t/p/w500/yQvGrMoipbRoddT0ZR8tPoR7NfX.jpg", 2014, "Interstellar" },
                    { 7, 0.0, "Ethan Hunt and his IMF team, along with some familiar allies, race against time after a mission goes wrong.", 0, "https://image.tmdb.org/t/p/w500/AkJQpZp9WoNdj7pLYSj1L0RcMMN.jpg", 2018, "Mission: Impossible - Fallout" },
                    { 8, 4.7999999999999998, "The Avengers and their allies must be willing to sacrifice all in an attempt to defeat the powerful Thanos before his blitz of devastation and ruin puts an end to the universe.", 0, "https://image.tmdb.org/t/p/w500/7WsyChQLEftFiDOVTGkv3hFpyyt.jpg", 2018, "Avengers: Infinity War" },
                    { 9, 0.0, "Based on the true story of Jordan Belfort, from his rise to a wealthy stock-broker living the high life to his fall involving crime, corruption and the federal government.", 0, "https://image.tmdb.org/t/p/w500/kW9LmvYHAaS9iA0tHmZVq8hQYoq.jpg", 2013, "The Wolf of Wall Street" },
                    { 10, 4.7000000000000002, "Young Blade Runner K's discovery of a long-buried secret leads him to track down former Blade Runner Rick Deckard, who's been missing for thirty years.", 0, "https://image.tmdb.org/t/p/w500/gajva2L0rPYkEWjzgFlBXCAVBE5.jpg", 2017, "Blade Runner 2049" },
                    { 11, 0.0, "The presidencies of Kennedy and Johnson, the Vietnam War, the Watergate scandal and other historical events unfold from the perspective of an Alabama man with an IQ of 75.", 0, "https://image.tmdb.org/t/p/w500/Cw4hIUIAmSYfK9QfaUW5igp9La.jpg", 1994, "Forrest Gump" },
                    { 12, 0.0, "A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.", 0, "https://image.tmdb.org/t/p/w500/wN2xWp1eIwCKOD0BHTcErTBv1Uq.jpg", 2000, "Gladiator" },
                    { 13, 0.0, "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.", 0, "https://image.tmdb.org/t/p/w500/dXNAPwY7VrqMAo51EKhhCJfaGb5.jpg", 1999, "The Matrix" },
                    { 14, 4.9000000000000004, "Allied soldiers from Belgium, the British Empire and France are surrounded by the German Army, and evacuated during a fierce battle in World War II.", 0, "https://image.tmdb.org/t/p/w500/ebSnODDg9lbsMIaWg2uAbjn7TO5.jpg", 2017, "Dunkirk" }
                });

            migrationBuilder.InsertData(
                table: "UserSpinStates",
                columns: new[] { "UserId", "BonusSpins", "DailySpinsRemaining", "EventSpinRewardsToday", "LastLoginDate", "LastSlotSpinReset", "LastTriviaSpinReset", "LoginStreak" },
                values: new object[] { 1, 2, 5, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.UpdateData(
                table: "UserStats",
                keyColumn: "StatsId",
                keyValue: 1,
                columns: new[] { "TotalPoints", "WeeklyScore" },
                values: new object[] { 1500, 200 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthProvider", "AuthSubject", "Username" },
                values: new object[,]
                {
                    { 2, "local", "beta", "UserBeta" },
                    { 3, "local", "gamma", "UserGamma" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "MessageId", "AuthorId", "Content", "CreatedAt", "MovieId", "ParentCommentId" },
                values: new object[] { 1, 2, "I can't wait for the sequel!", new DateTime(2026, 5, 11, 15, 38, 52, 100, DateTimeKind.Utc).AddTicks(2324), 1, null });

            migrationBuilder.InsertData(
                table: "MovieActors",
                columns: new[] { "ActorsId", "MovieId" },
                values: new object[,]
                {
                    { 1, 7 },
                    { 2, 8 },
                    { 3, 3 },
                    { 3, 6 },
                    { 4, 11 }
                });

            migrationBuilder.InsertData(
                table: "MovieDirectors",
                columns: new[] { "DirectorsId", "MovieId" },
                values: new object[,]
                {
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 6 },
                    { 2, 14 },
                    { 3, 9 },
                    { 4, 4 }
                });

            migrationBuilder.InsertData(
                table: "MovieGenres",
                columns: new[] { "GenresId", "MovieId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 1, 5 },
                    { 1, 7 },
                    { 1, 8 },
                    { 1, 12 },
                    { 3, 1 },
                    { 3, 9 },
                    { 3, 11 },
                    { 3, 12 },
                    { 3, 14 },
                    { 6, 3 },
                    { 6, 5 },
                    { 6, 6 },
                    { 6, 10 },
                    { 6, 13 },
                    { 7, 2 },
                    { 7, 4 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "ReviewId", "ActingRating", "ActingText", "CgiRating", "CgiText", "CinematographyRating", "CinematographyText", "Content", "CreatedAt", "IsExtraReview", "MovieId", "PlotRating", "PlotText", "SoundRating", "SoundText", "StarRating", "UserId" },
                values: new object[,]
                {
                    { 1, 5, null, 4, null, 5, null, "A masterpiece of modern cinema.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8291), true, 1, 5, null, 5, null, 4.5f, 2 },
                    { 2, 0, null, 0, null, 0, null, "Decent, but could be better.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8294), false, 1, 0, null, 0, null, 3f, 3 },
                    { 3, 0, null, 0, null, 0, null, "Visually stunning and immersive.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8295), false, 5, 0, null, 0, null, 5f, 2 },
                    { 4, 0, null, 0, null, 0, null, "The ultimate Marvel event.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8296), false, 8, 0, null, 0, null, 4.8f, 3 },
                    { 5, 0, null, 0, null, 0, null, "A perfect sequel to a classic.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8297), false, 10, 0, null, 0, null, 4.7f, 2 },
                    { 6, 0, null, 0, null, 0, null, "Nolan's best work yet.", new DateTime(2026, 5, 11, 20, 38, 52, 118, DateTimeKind.Utc).AddTicks(8297), false, 14, 0, null, 0, null, 4.9f, 3 }
                });

            migrationBuilder.InsertData(
                table: "UserSpinStates",
                columns: new[] { "UserId", "BonusSpins", "DailySpinsRemaining", "EventSpinRewardsToday", "LastLoginDate", "LastSlotSpinReset", "LastTriviaSpinReset", "LoginStreak" },
                values: new object[,]
                {
                    { 2, 1, 3, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0 },
                    { 3, 0, 1, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0 }
                });

            migrationBuilder.InsertData(
                table: "UserStats",
                columns: new[] { "StatsId", "TotalPoints", "UserId", "WeeklyScore" },
                values: new object[,]
                {
                    { 2, 450, 2, 50 },
                    { 3, 100, 3, 10 }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "MessageId", "AuthorId", "Content", "CreatedAt", "MovieId", "ParentCommentId" },
                values: new object[] { 2, 3, "Agreed, the ending was insane.", new DateTime(2026, 5, 11, 18, 38, 52, 100, DateTimeKind.Utc).AddTicks(2330), 1, 1 });

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "SeatBookings");

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "BadgeId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Comments",
                keyColumn: "MessageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorsId", "MovieId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorsId", "MovieId" },
                keyValues: new object[] { 2, 8 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorsId", "MovieId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorsId", "MovieId" },
                keyValues: new object[] { 3, 6 });

            migrationBuilder.DeleteData(
                table: "MovieActors",
                keyColumns: new[] { "ActorsId", "MovieId" },
                keyValues: new object[] { 4, 11 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 2, 6 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 2, 14 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 3, 9 });

            migrationBuilder.DeleteData(
                table: "MovieDirectors",
                keyColumns: new[] { "DirectorsId", "MovieId" },
                keyValues: new object[] { 4, 4 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 1, 8 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 1, 12 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 3, 9 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 3, 11 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 3, 12 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 3, 14 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 6, 5 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 6, 6 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 6, 10 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 6, 13 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenresId", "MovieId" },
                keyValues: new object[] { 7, 4 });

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserSpinStates",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserSpinStates",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserSpinStates",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserStats",
                keyColumn: "StatsId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserStats",
                keyColumn: "StatsId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Actors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Comments",
                keyColumn: "MessageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Directors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Badges");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Movies",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "UserStats",
                keyColumn: "StatsId",
                keyValue: 1,
                columns: new[] { "TotalPoints", "WeeklyScore" },
                values: new object[] { 1000, 50 });

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
