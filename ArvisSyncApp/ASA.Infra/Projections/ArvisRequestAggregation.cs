using ASA.Domain;
using ASA.Domain.Interfaces;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace ASA.Infra.Projections;

/// <summary>
///     write model
/// </summary>
public class ArvisRequestAggregation : SingleStreamProjection<ArvisRequest, Guid>
{
    public override ArvisRequest? Evolve(ArvisRequest? snapshot, Guid id, IEvent e) => e.Data is IDomainEvent @event
        ? ArvisRequest.Evolve(snapshot, @event, e.Version)
        : snapshot;
}