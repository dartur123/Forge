using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Forge.Infrastructure;

public class ForgeDbContextFactory : IDesignTimeDbContextFactory<ForgeDbContext>
{
    public ForgeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets("1ff7be08-210a-43dc-a881-9525f8a8a496")
            .AddEnvironmentVariables()
            .Build();

        // Falls back to the local docker-compose Postgres credentials (already public in
        // docker-compose.yml) when user secrets can't be resolved by the design-time host,
        // e.g. Visual Studio's Package Manager Console process.
        var connectionString = configuration.GetConnectionString("ForgeDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Port=5432;Database=forge_db;Username=forge_user;Password=forge_dev_password";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ForgeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ForgeDbContext(optionsBuilder.Options);
    }
}
