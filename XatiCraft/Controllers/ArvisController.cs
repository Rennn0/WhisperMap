using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace XatiCraft.Controllers;

/// <inheritdoc />
[ApiController]
[Route("[controller]")]
public class ArvisController : ApplicationController
{
    /// <summary>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IResult> CreateArvisRequest([FromBody] CreateArvisRequest request,
        [FromServices] ICommandHandler<CreateArvisRequest, long> handler, CancellationToken cancellationToken)
    {
        long version = await handler.HandleAsync(request, cancellationToken);
        HttpContext.Request.Headers.ETag = version.ToString();
        return Results.Created($"{version}", new { request.Id, version });
    }
}