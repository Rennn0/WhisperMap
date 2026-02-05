using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XatiCraft.Data;

/// <inheritdoc />
public class ApplicationContextFactory 
    : IDesignTimeDbContextFactory<ApplicationContext>
{
    /// <inheritdoc />
    public ApplicationContext CreateDbContext(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = config.GetConnectionString(nameof(ApplicationContext));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string {nameof(ApplicationContext)} not found.");

        DbContextOptionsBuilder<ApplicationContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationContext(optionsBuilder.Options);
    }
}