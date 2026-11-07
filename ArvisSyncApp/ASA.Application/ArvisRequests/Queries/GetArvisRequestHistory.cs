using ASA.Application.Interfaces;

namespace ASA.Application.ArvisRequests.Queries;

public record GetArvisRequestHistory(Guid Id, long FromVersion) : IQuery<ArvisRequestWithHistory>;