using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;

namespace XcLib.Data.Postgres.XatiCraft;

public abstract class RootRepo<TModel> where TModel : class
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    /// <summary>
    ///     implementations using Npgsql.EntityFrameworkCore.PostgreSQL Version=8.0.0
    /// </summary>
    protected RootRepo(IDbContextFactory<ApplicationContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    protected async Task<T> ExecuteAsync<T>(
        Func<DbSet<TModel>, CancellationToken, Task<T>> operation,
        CancellationToken token = default)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(token);
        T result = await operation(context.Set<TModel>(), token);
        await context.SaveChangesAsync(token);
        return result;
    }

    protected async Task<T> ExecuteTransactionAsync<T>(
        Func<ApplicationContext, CancellationToken, Task<T>> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted,
        CancellationToken token = default)
    {
        await using ApplicationContext context =
            await _dbContextFactory.CreateDbContextAsync(token);

        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            (Context: context, Operation: operation, IsolationLevel: isolationLevel),
            static async (state, ct) =>
            {
                await using IDbContextTransaction transaction =
                    await state.Context.Database.BeginTransactionAsync(state.IsolationLevel, ct);

                try
                {
                    T result = await state.Operation(state.Context, ct);
                    await transaction.CommitAsync(ct);

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            },
            token);
    }

    protected abstract Expression<Func<TModel, bool>> ToSearchPredicate(ApplicationObject obj, ushort searchFlag); 
}