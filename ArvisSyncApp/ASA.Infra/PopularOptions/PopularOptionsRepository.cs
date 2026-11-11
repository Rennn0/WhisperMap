using ASA.Domain;
using ASA.Domain.Interfaces;
using Marten;

namespace ASA.Infra.PopularOptions;

internal sealed class PopularOptionsRepository : IRepository<PopularRequestOption>
{
    private readonly IDocumentSession _session;

    public PopularOptionsRepository(IDocumentSession session) => _session = session;

    public Task<PopularRequestOption?> GetAsync(object id, CancellationToken cancellationToken = default) =>
        _session.LoadAsync<PopularRequestOption>(id, cancellationToken);

    public async Task<long> SaveAsync(PopularRequestOption request, CancellationToken cancellationToken = default)
    {
        _session.Store(request);
        _session.CorrelationId = "ratatui";
        await _session.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}