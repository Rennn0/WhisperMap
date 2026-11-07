using ASA.Domain;
using ASA.Domain.Interfaces;
using JasperFx;
using Marten;
using Marten.Events;
using Marten.Exceptions;

namespace ASA.Infra;

internal sealed class MartenArvisRequestRepository : IArvisRequestRepository
{
    private readonly IDocumentSession _documentSession;

    private readonly Dictionary<Guid, IEventStream<ArvisRequest>> _streams =
        new Dictionary<Guid, IEventStream<ArvisRequest>>();

    public MartenArvisRequestRepository(IDocumentSession documentSession) => _documentSession = documentSession;

    public async Task<ArvisRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IEventStream<ArvisRequest> stream =
            await _documentSession.Events.FetchForWriting<ArvisRequest>(id, cancellationToken);
        _streams[id] = stream;
        return stream.Aggregate;
    }

    public async Task<long> SaveAsync(ArvisRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UncommitedDomainEvents.Count == 0) return request.Version;
        try
        {
            if (!_streams.TryGetValue(request.Id, out IEventStream<ArvisRequest>? stream))
            {
                stream = await _documentSession.Events.FetchForWriting<ArvisRequest>(request.Id, request.Version,
                    cancellationToken);
                _streams[request.Id] = stream;
            }

            stream.AppendMany(request.UncommitedDomainEvents);
            await _documentSession.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e) when (e is ConcurrencyException or ExistingStreamIdCollisionException)
        {
            throw new Domain.Exceptions.ConcurrencyException(request.Id, request.Version, null, e);
        }

        request.MarkCommited();
        return request.Version;
    }
}