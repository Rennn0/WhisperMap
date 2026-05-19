using Microsoft.AspNetCore.Mvc.Filters;
using XatiCraft.Guards;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;

namespace XatiCraft.Controllers.Attributes.Filters;

internal class LogVisitorFilter : IAsyncActionFilter
{
    private readonly IPageVisitorRepo _pageVisitorRepo;

    public LogVisitorFilter(IPageVisitorRepo pageVisitorRepo) => _pageVisitorRepo = pageVisitorRepo;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        HttpContext http = context.HttpContext;
        PageVisitor pv = new PageVisitor(http.Request.Path, http.Request.Headers[AuthGuard.IpHeader],
            http.Request.Cookies[AuthGuard.UserIdCookie], http.Request.Headers.UserAgent);
        _ = _pageVisitorRepo.AddAsync(pv, context.HttpContext.RequestAborted).ConfigureAwait(false);

        await next();
    }
}