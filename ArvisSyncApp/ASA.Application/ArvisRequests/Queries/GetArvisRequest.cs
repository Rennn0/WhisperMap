using ASA.Application.Interfaces;

namespace ASA.Application.ArvisRequests.Queries;

public sealed record GetArvisRequest(Guid Id) : IQuery<ArvisRequestView?>;