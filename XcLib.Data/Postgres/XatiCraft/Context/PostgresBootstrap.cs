using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XcLib.Data.Abstractions;

namespace XcLib.Data.Postgres.XatiCraft.Context;

public class PostgresBootstrap : IBootstrap
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PostgresBootstrap(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async ValueTask RunAsync()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ILogger<PostgresBootstrap> logger = scope.ServiceProvider.GetRequiredService<ILogger<PostgresBootstrap>>();
        ApplicationContext db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("migration applied");
    }

    public void Run() => throw new NotImplementedException();
}