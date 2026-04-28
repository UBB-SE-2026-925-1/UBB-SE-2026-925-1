using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovieApp.Infrastructure;

public class MovieAppDbContextFactory : IDesignTimeDbContextFactory<MovieAppDbContext>
{
    public MovieAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MovieAppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MovieAppUnified;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new MovieAppDbContext(optionsBuilder.Options);
    }
}
