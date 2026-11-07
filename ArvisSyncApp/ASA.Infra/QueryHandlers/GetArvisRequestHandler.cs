using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using Marten;

namespace ASA.Infra.QueryHandlers;

internal sealed class GetArvisRequestHandler : IQueryHandler<GetArvisRequest, ArvisRequestView?>
{
    private readonly IQuerySession _querySession;

    public GetArvisRequestHandler(IQuerySession querySession) => _querySession = querySession;

    public Task<ArvisRequestView?> HandleAsync(GetArvisRequest command, CancellationToken cancellationToken) =>
        _querySession.LoadAsync<ArvisRequestView>(command.Id, cancellationToken);
}