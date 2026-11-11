using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.Queries;
using ASA.Domain;
using Marten;

namespace ASA.Infra.PopularOptions.QueryHandlers;

internal class ListPopularOptionHandler : IQueryHandler<ListPopularOptions, IReadOnlyList<PopularRequestOption>>
{
    private readonly IQuerySession _session;

    public ListPopularOptionHandler(IQuerySession session) => _session = session;

    public Task<IReadOnlyList<PopularRequestOption>> HandleAsync(ListPopularOptions query,
        CancellationToken cancellationToken)
    {
        return _session.Query<PopularRequestOption>()
            .Where(x => query.ConfigPlace == null || x.Place == query.ConfigPlace)
            .ToListAsync(cancellationToken);
    }
}