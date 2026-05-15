using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcLib.Data.Postgres.XatiCraft.Context;

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args) =>
        new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("CONN"),
                opt => { opt.EnableRetryOnFailure(); })
            .Options);
}

// ef migrations add --project XcLib.Data\XcLib.Data.csproj --startup-project XatiCraft\XatiCraft.csproj --context XcLib.Data.Postgres.XatiCraft.Context.ApplicationContext --configuration Debug <NAME> --output-dir Postgres\XatiCraft\Migrations

// CONN="<CONNECTION>" \
// dotnet ef database update \
// --project "XcLib.Data/XcLib.Data.csproj" \
// --startup-project "XatiCraft/XatiCraft.csproj" \
// --context "XcLib.Data.Postgres.XatiCraft.Context.ApplicationContext"
