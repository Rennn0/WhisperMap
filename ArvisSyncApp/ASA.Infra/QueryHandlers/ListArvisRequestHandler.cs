using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using Marten;

namespace ASA.Infra.QueryHandlers;

internal sealed class ListArvisRequestHandler : IQueryHandler<ListArvisRequest, IReadOnlyList<ArvisRequestView>>
{
    private readonly IQuerySession _querySession;

    public ListArvisRequestHandler(IQuerySession querySession) => _querySession = querySession;

    public Task<IReadOnlyList<ArvisRequestView>> HandleAsync(ListArvisRequest query,
        CancellationToken cancellationToken) => _querySession.Query<ArvisRequestView>()
        .Where(v => !v.IsDeleted)
        .Skip(Math.Max(query.Skip, 0))
        .Take(Math.Clamp(query.Take, 1, ListArvisRequest.MaxTake))
        .ToListAsync(cancellationToken);
}