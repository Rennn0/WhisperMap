using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcLib.Data.Postgres.XatiCraft.Context;

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args) =>
        new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("CON"),
                opt => { opt.EnableRetryOnFailure(); })
            .Options);
}