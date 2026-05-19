using Microsoft.AspNetCore.Mvc;
using XatiCraft.Controllers.Attributes.Filters;

namespace XatiCraft.Controllers.Attributes;

internal class PageVisitorAttribute : TypeFilterAttribute
{
    public PageVisitorAttribute() : base(typeof(LogVisitorFilter))
    {
    }
}