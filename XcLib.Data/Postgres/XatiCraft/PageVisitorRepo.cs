using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;

namespace XcLib.Data.Postgres.XatiCraft;

public class PageVisitorRepo : IPageVisitorRepo
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    public PageVisitorRepo(IDbContextFactory<ApplicationContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    public async Task<PageVisitor> AddAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(token);
        PageVisitor pv = new PageVisitor
        {
            IpAddress = obj.IpAddress,
            Browser = obj.Browser,
            Page = obj.Page,
            Uid = obj.Uid
        };
        await context.PageVisitors.AddAsync(pv, token);
        await context.SaveChangesAsync(token);
        return pv;
    }

    public async Task<PageVisitor?> GetAsync(ApplicationObjects.PageVisitor obj, ushort searchFlag = 0,
        CancellationToken token = default)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(token);
        Expression<Func<PageVisitor, bool>> predicate = searchFlag switch
        {
            _ => pv => pv.Id == obj.Id
        };
        return await context.PageVisitors.AsNoTracking().FirstOrDefaultAsync(predicate, token);
    }

    public Task<PageVisitor?> UpdateAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default) =>
        throw new NotImplementedException();
}