using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Commands;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASA.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ArvisController : ControllerBase
{
    [HttpGet]
    public async Task<IResult> ListArvisRequests([FromQuery] int? skip, [FromQuery] int? take,
        [FromServices] IQueryHandler<ListArvisRequest, IReadOnlyList<ArvisRequestView>> handler,
        CancellationToken cancellationToken) =>
        Results.Json(await handler.HandleAsync(new ListArvisRequest(skip ?? 0, take ?? 50), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetArvisRequest([FromRoute] Guid id, [FromQuery] long? fromVersion,
        [FromServices] IQueryHandler<GetArvisRequestHistory, ArvisRequestWithHistory> handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(new GetArvisRequestHistory(id, fromVersion ?? 0), cancellationToken));

    [HttpPost]
    public async Task<IResult> CreateArvisRequest([FromBody] CreateArvisRequest request,
        [FromServices] ICommandHandler<CreateArvisRequest, long> handler, CancellationToken cancellationToken)
    {
        long version = await handler.HandleAsync(request, cancellationToken);
        HttpContext.Request.Headers.ETag = version.ToString();
        return Results.Created($"{version}", new { request.Id, version });
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IResult> UpdateArvisRequest([FromRoute] Guid id, [FromHeader(Name = "If-Match")] string ifMatch,
        [FromBody] UpdateArvisRequest request,
        [FromServices] ICommandHandler<UpdateArvisRequest, long> handler, CancellationToken cancellationToken)
    {
        if (!long.TryParse(ifMatch, out long expected)) return Results.BadRequest(nameof(ifMatch));
        
        long version = await handler.HandleAsync(
            request with { Id = id, ExpectedVersion = expected }, cancellationToken);
        HttpContext.Request.Headers.ETag = version.ToString();
        return Results.NoContent();
    }
}