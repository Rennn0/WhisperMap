using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XcLib.Data.Abstractions;

namespace XcLib.Data.SqlServer.Realtime.Context;

public class SqlServerBootstrap : IBootstrap
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SqlServerBootstrap(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async ValueTask RunAsync()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ILogger<SqlServerBootstrap> logger = scope.ServiceProvider.GetRequiredService<ILogger<SqlServerBootstrap>>();
        MasterDbContext db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("migration applied");
    }

    public void Run() => throw new NotImplementedException();
}