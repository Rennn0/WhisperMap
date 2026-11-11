using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using Marten;

namespace ASA.Infra.ArvisRequests.QueryHandlers;

internal sealed class GetArvisRequestHandler : IQueryHandler<GetArvisRequest, ArvisRequestView?>
{
    private readonly IQuerySession _querySession;

    public GetArvisRequestHandler(IQuerySession querySession) => _querySession = querySession;

    public async Task<ArvisRequestView?> HandleAsync(GetArvisRequest query, CancellationToken cancellationToken)
    {
        ArvisRequestView? view = await _querySession.LoadAsync<ArvisRequestView>(query.Id, cancellationToken);
        return view is { IsDeleted: false } ? view : null;
    }
}