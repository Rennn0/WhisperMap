using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;
using PageVisitor = XcLib.Data.Postgres.XatiCraft.Model.PageVisitor;

namespace XcLib.Data.Postgres.XatiCraft;

public class PageVisitorRepo : RootRepo<PageVisitor>, IPageVisitorRepo
{
    /// <inheritdoc />
    public PageVisitorRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<ApplicationObjects.PageVisitor> AddAsync(ApplicationObjects.PageVisitor obj,
        CancellationToken token = default) =>
        await ExecuteAsync(async (pageVisitors, cancellationToken) =>
        {
            PageVisitor pv = new PageVisitor
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

    public async Task<ApplicationObjects.PageVisitor> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteAsync(
            async (pageVisitors, cancellationToken) => ApplicationObjects.PageVisitor.From(await pageVisitors
                .AsNoTracking()
                .SingleAsync(pv => pv.Id == id, cancellationToken)), token);

    public async Task<List<ApplicationObjects.PageVisitor>> GetAsync(ApplicationObjects.PageVisitor obj,
        sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(async (pageVisitors, cancellationToken) =>
        {
            List<PageVisitor> entities = await pageVisitors.Where(ToSearchPredicate(obj, searchFlag))
                .ToListAsync(cancellationToken);
            return entities.Select(ApplicationObjects.PageVisitor.From).ToList();
        }, token);


    public async Task<ApplicationObjects.PageVisitor?> UpdateAsync(ApplicationObjects.PageVisitor obj,
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

    public async Task<int> DeleteAsync(ApplicationObjects.PageVisitor obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteTransactionAsync(
            (context, cancellationToken) => context.PageVisitors
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteDeleteAsync(cancellationToken),
            token: token);

    public async Task<bool> ExistsAsync(ApplicationObjects.PageVisitor obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(
            (pageVisitors, cancellationToken) => pageVisitors.AsNoTracking()
                .AnyAsync(ToSearchPredicate(obj, searchFlag), cancellationToken), token);

    protected override Expression<Func<PageVisitor, bool>> ToSearchPredicate(ApplicationObject obj,
        sbyte searchFlag)
    {
        if (obj is not ApplicationObjects.PageVisitor pvObj) throw new ArgumentOutOfRangeException(nameof(obj));
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