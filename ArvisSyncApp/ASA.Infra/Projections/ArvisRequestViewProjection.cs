using ASA.Application.ArvisRequests;
using ASA.Domain.Events;
using ASA.Domain.Interfaces;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace ASA.Infra.Projections;

public class ArvisRequestViewProjection : SingleStreamProjection<ArvisRequestView, Guid>
{
    public override ArvisRequestView? Evolve(ArvisRequestView? snapshot, Guid id, IEvent e)
    {
        if (e.Data is ArvisRequestCreated created)
            return new ArvisRequestView
            {
                Id = id, Title = created.Title,
                Headers = [.. created.Headers], Cookies = [.. created.Cookies],
                CreatedAt = created.OccurredAt, UpdatedAt = created.OccurredAt
            };

        if (snapshot is null || e.Data is not IDomainEvent domainEvent)
            return snapshot;

        switch (domainEvent)
        {
            case ArvisRequestRenamed x: snapshot.Title = x.NewTitle; break;
            case ArvisRequestHeaderReplaced x: snapshot.Headers = [.. x.Headers]; break;
            case ArvisRequestCookieReplaced x: snapshot.Cookies = [.. x.Cookies]; break;
            case ArvisRequestDeleted: snapshot.IsDeleted = true; break;
        }

        snapshot.UpdatedAt = domainEvent.OccurredAt;
        return snapshot;
    }
}