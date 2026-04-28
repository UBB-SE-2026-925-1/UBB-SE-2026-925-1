using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthProvider", "AuthSubject", "Username" },
                values: new object[] { 1, "local", "admin", "AdminUser" });

            migrationBuilder.InsertData(
                table: "UserStats",
                columns: new[] { "StatsId", "TotalPoints", "UserId", "WeeklyScore" },
                values: new object[] { 1, 1000, 1, 50 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserStats",
                keyColumn: "StatsId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
