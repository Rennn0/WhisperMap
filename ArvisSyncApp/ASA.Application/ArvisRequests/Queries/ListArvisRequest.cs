using ASA.Application.Interfaces;

namespace ASA.Application.ArvisRequests.Queries;

public sealed record ListArvisRequest(int Skip = 0, int Take = 50) : IQuery<IReadOnlyList<ArvisRequestView>>
{
    public const int MaxTake = 250;
}