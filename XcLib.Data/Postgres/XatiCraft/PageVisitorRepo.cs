using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;

namespace XcLib.Data.Postgres.XatiCraft;

public class PageVisitorRepo : RootRepo<PageVisitorModel>, IPageVisitorRepo
{
    /// <inheritdoc />
    public PageVisitorRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<PageVisitor> AddAsync(PageVisitor obj,
        CancellationToken token = default) =>
        await ExecuteAsync(async (pageVisitors, cancellationToken) =>
        {
            PageVisitorModel pv = new PageVisitorModel
            {
                IpAddress = obj.IpAddress,
                Browser = obj.Browser,
                Page = obj.Page,
                Uid = obj.Uid
            };
            await pageVisitors.AddAsync(pv, cancellationToken);
            obj.Id = pv.Id;
            return obj;
        }, token);

    public async Task<PageVisitor> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteAsync(
            async (pageVisitors, cancellationToken) => PageVisitor.From(await pageVisitors
                .AsNoTracking()
                .SingleAsync(pv => pv.Id == id, cancellationToken)), token);

    public async Task<List<PageVisitor>> GetAsync(PageVisitor obj,
        sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(async (pageVisitors, cancellationToken) =>
        {
            List<PageVisitorModel> entities = await pageVisitors.Where(ToSearchPredicate(obj, searchFlag))
                .ToListAsync(cancellationToken);
            return entities.Select(PageVisitor.From).ToList();
        }, token);


    public async Task<PageVisitor?> UpdateAsync(PageVisitor obj,
        sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteTransactionAsync(async (context, cancellationToken) =>
        {
            await context.PageVisitors
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteUpdateAsync(calls =>
                        calls
                            .SetProperty(p => p.Page, obj.Page)
                            .SetProperty(p => p.Uid, obj.Uid)
                            .SetProperty(p => p.Browser, obj.Browser)
                            .SetProperty(p => p.IpAddress, obj.IpAddress)
                    , cancellationToken);
            return obj;
        }, token: token);

    public async Task<int> DeleteAsync(PageVisitor obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteTransactionAsync(
            (context, cancellationToken) => context.PageVisitors
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteDeleteAsync(cancellationToken),
            token: token);

    public async Task<bool> ExistsAsync(PageVisitor obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(
            (pageVisitors, cancellationToken) => pageVisitors.AsNoTracking()
                .AnyAsync(ToSearchPredicate(obj, searchFlag), cancellationToken), token);

    protected override Expression<Func<PageVisitorModel, bool>> ToSearchPredicate(ApplicationObject obj,
        sbyte searchFlag)
    {
        if (obj is not PageVisitor pvObj) throw new ArgumentOutOfRangeException(nameof(obj));
        return searchFlag switch
        {
            1 => p => p.IpAddress == pvObj.IpAddress,
            2 => p => p.Page == pvObj.Page,
            3 => p => p.Browser == pvObj.Browser,
            4 => p => p.Uid == pvObj.Uid,
            5 => p => p.Page == pvObj.Page && p.IpAddress == pvObj.IpAddress,
            6 => p => p.Page == pvObj.Page || p.IpAddress == pvObj.IpAddress,
            _ => p => p.Id == pvObj.Id
        };
    }
}