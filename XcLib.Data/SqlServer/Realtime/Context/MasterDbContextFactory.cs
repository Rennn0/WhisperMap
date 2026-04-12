using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XcLib.Data.SqlServer.Realtime.Context;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args) => new MasterDbContext(
        new DbContextOptionsBuilder<MasterDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("CON"),
                x => x.EnableRetryOnFailure())
            .Options);
}

// CON="Server=localhost;Database=master;User Id=sa;Password=Test123test;Trusted_Connection=False;Persist Security Info=False;Encrypt=False" dotnet ef migrations add --project XcLib.Data/XcLib.Data.csproj --startup-project Realtime/Realtime.csproj --context XcLib.Data.SqlServer.Realtime.Context.MasterDbContext --configuration Debug --verbose Initial --output-dir SqlServer/Realtime/Migrations
// dotnet ef database update --project XcLib.Data/XcLib.Data.csproj --startup-project Realtime/Realtime.csproj --context XcLib.Data.SqlServer.Realtime.Context.MasterDbContext --configuration Debug --verbose 20260412175233_Initial --connection "Server=localhost;Database=master;User Id=sa;Password=Test123test;Trusted_Connection=False;Persist Security Info=False;Encrypt=False