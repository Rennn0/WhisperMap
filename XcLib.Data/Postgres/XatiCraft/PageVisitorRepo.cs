using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;

namespace XcLib.Data.Postgres.XatiCraft;

public class PageVisitorRepo : IPageVisitorRepo
{
    private readonly DbSet<PageVisitor> _pageVisitors;
    private readonly ApplicationContext _context;

    public PageVisitorRepo(ApplicationContext context)
    {
        _context = context;
        _pageVisitors = context.PageVisitors;
    }

    public async Task<PageVisitor> AddAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default)
    {
        PageVisitor pv = new PageVisitor
        {
            IpAddress = obj.IpAddress,
            Browser = obj.Browser,
            Page = obj.Page,
            Uid = obj.Uid
        };
        await _pageVisitors.AddAsync(pv, token);
        await _context.SaveChangesAsync(token);
        return pv;
    }

    public Task<PageVisitor?> GetAsync(ApplicationObjects.PageVisitor obj, ushort searchFlag = 0,
        CancellationToken token = default)
    {
        Expression<Func<PageVisitor, bool>> predicate = searchFlag switch
        {
            _ => pv => pv.Id == obj.Id
        };
        return _pageVisitors.AsNoTracking().FirstOrDefaultAsync(predicate, token);
    }

    public Task<PageVisitor?> UpdateAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(ApplicationObjects.PageVisitor obj, CancellationToken token = default) =>
        throw new NotImplementedException();
}