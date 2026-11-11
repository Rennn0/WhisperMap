using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.Queries;
using ASA.Domain;
using Marten;

namespace ASA.Infra.PopularOptions.QueryHandlers;

internal class GetPopularOptionHandler : IQueryHandler<GetPopularOption, PopularRequestOption?>
{
    private readonly IQuerySession _session;

    public GetPopularOptionHandler(IQuerySession session) => _session = session;

    public async Task<PopularRequestOption?> HandleAsync(GetPopularOption query, CancellationToken cancellationToken) =>
        await _session.LoadAsync<PopularRequestOption>(query.Id, cancellationToken);
}