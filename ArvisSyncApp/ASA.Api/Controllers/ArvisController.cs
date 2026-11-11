using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Commands;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.Commands;
using ASA.Application.PopularOptions.Queries;
using ASA.Domain;
using ASA.Domain.Enums;
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

    [HttpGet("options/{id:long}")]
    public async Task<IResult> GetArvisOption(
        [FromServices] IQueryHandler<GetPopularOption, PopularRequestOption?> handler, long id) =>
        Results.Ok(await handler.HandleAsync(new GetPopularOption(id), CancellationToken.None));

    [HttpGet("options")]
    public async Task<IResult> GetArvisOptions([FromQuery] ConfigPlace? place,
        [FromServices] IQueryHandler<ListPopularOptions, IReadOnlyList<PopularRequestOption>> handler) =>
        Results.Ok(await handler.HandleAsync(new ListPopularOptions(place), CancellationToken.None));

    [HttpPost("options")]
    public async Task<IResult> GetArvisOptions([FromBody] PopularRequestOption option,
        [FromServices] ICommandHandler<CreatePopularOption, long> repository, CancellationToken cancellationToken) =>
        Results.Ok(await repository.HandleAsync(new CreatePopularOption(option), cancellationToken));
}