namespace ASA.Application.ArvisRequests;

public sealed record ArvisRequestWithHistory(
    ArvisRequestView? State,
    IReadOnlyList<ArvisRequestWithHistory.HistoryItem> History)
{
    public sealed record HistoryItem(long Version, string Type, DateTimeOffset CreatedAt, object Data);
}