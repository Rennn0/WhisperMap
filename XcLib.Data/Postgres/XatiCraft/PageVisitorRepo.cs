using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;

namespace XcLib.Data.Postgres.XatiCraft;

public class PageVisitorRepo : RootRepo, IPageVisitorRepo
{
    /// <inheritdoc />
    public PageVisitorRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<PageVisitor> AddAsync(PageVisitor obj,
        CancellationToken token = default) =>
        await ExecuteAsync(async (context, cancellationToken) =>
        {
            Model.PageVisitor pv = new Model.PageVisitor
            {
                IpAddress = obj.IpAddress,
                Browser = obj.Browser,
                Page = obj.Page,
                Uid = obj.Uid
            };
            await context.PageVisitors.AddAsync(pv, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            obj.Id = pv.Id;
            return obj;
        }, token);

    public async Task<PageVisitor?> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteAsync(async (context, cancellationToken) =>
        {
            Model.PageVisitor? pv =
                await context.PageVisitors.AsNoTracking().FirstOrDefaultAsync(pv => pv.Id == id, cancellationToken);
            return null == pv ? null : PageVisitor.From(pv);
        }, token);

    public async Task<List<PageVisitor>> GetAsync(PageVisitor obj, ushort searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(async (context, cancellationToken) =>
        {
            Expression<Func<Model.PageVisitor, bool>> predicate = searchFlag switch
            {
                1 => p => p.IpAddress == obj.IpAddress,
                2 => p => p.Page == obj.Page,
                3 => p => p.Browser == obj.Browser,
                _ => pv => pv.Id == obj.Id
            };

            List<Model.PageVisitor> entities = await context.PageVisitors.Where(predicate)
                .ToListAsync(cancellationToken);
            return entities.Select(PageVisitor.From).ToList();
        }, token);


    public Task<PageVisitor?> UpdateAsync(PageVisitor obj,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(PageVisitor obj, CancellationToken token = default) =>
        throw new NotImplementedException();
}