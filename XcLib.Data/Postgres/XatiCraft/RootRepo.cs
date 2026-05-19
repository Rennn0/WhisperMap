using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using XcLib.Data.Postgres.XatiCraft.Context;

namespace XcLib.Data.Postgres.XatiCraft;

public abstract class RootRepo
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    /// <summary>
    ///     implementations using Npgsql.EntityFrameworkCore.PostgreSQL Version=8.0.0
    /// </summary>
    protected RootRepo(IDbContextFactory<ApplicationContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    protected async Task<T> ExecuteAsync<T>(
        Func<ApplicationContext, CancellationToken, Task<T>> operation,
        CancellationToken token = default)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(token);
        return await operation(context, token);
    }

    protected async Task<T> ExecuteTransactionAsync<T>(
        Func<ApplicationContext, CancellationToken, Task<T>> operation,
        CancellationToken token = default)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(token);
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(token);
        try
        {
            T result = await operation(context, token);
            await transaction.CommitAsync(token);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
}