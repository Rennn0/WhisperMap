using ASA.Application.ArvisRequests;
using ASA.Application.ArvisRequests.Queries;
using ASA.Application.Interfaces;
using JasperFx.Events;
using Marten;
using Marten.Services.BatchQuerying;

namespace ASA.Infra.QueryHandlers;

internal sealed class
    GetArvisRequestHistoryHandler : IQueryHandler<GetArvisRequestHistory, ArvisRequestWithHistory>
{
    private readonly IQuerySession _querySession;

    public GetArvisRequestHistoryHandler(IQuerySession querySession) => _querySession = querySession;

    public async Task<ArvisRequestWithHistory> HandleAsync(GetArvisRequestHistory query,
        CancellationToken cancellationToken)
    {
        IBatchedQuery batch = _querySession.CreateBatchQuery();
        Task<ArvisRequestView?> stateTask = batch.Load<ArvisRequestView>(query.Id);
        Task<IReadOnlyList<IEvent>> historyTask = batch.Events.FetchStream(query.Id, fromVersion: query.FromVersion);
        await batch.Execute(cancellationToken);
        ArvisRequestView? state = await stateTask;
        IReadOnlyList<IEvent> history = await historyTask;
        return new ArvisRequestWithHistory(state,
        [
            .. history.Select(e =>
                new ArvisRequestWithHistory.HistoryItem(e.Version, e.EventTypeName, e.Timestamp, e.Data))
        ]);
    }
}