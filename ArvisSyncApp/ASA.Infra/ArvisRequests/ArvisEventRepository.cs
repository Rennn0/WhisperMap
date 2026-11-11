using ASA.Domain;
using ASA.Domain.Exceptions;
using ASA.Domain.Interfaces;
using Marten;
using Marten.Events;
using Marten.Exceptions;
using ConcurrencyException = JasperFx.ConcurrencyException;

namespace ASA.Infra.ArvisRequests;

internal sealed class ArvisEventRepository : IEventRepository<ArvisRequest>
{
    private readonly IDocumentSession _documentSession;

    private readonly Dictionary<Guid, IEventStream<ArvisRequest>> _streams =
        new Dictionary<Guid, IEventStream<ArvisRequest>>();

    public ArvisEventRepository(IDocumentSession documentSession) => _documentSession = documentSession;

    public async Task<ArvisRequest?> GetAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is not Guid idGuid) throw new DomainException("expected id type Guid");
        IEventStream<ArvisRequest> stream =
            await _documentSession.Events.FetchForWriting<ArvisRequest>(idGuid, cancellationToken);
        _streams[idGuid] = stream;
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