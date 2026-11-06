using ASA.Application.Interfaces;

namespace ASA.Application.ArvisRequests.Commands;

public sealed record DeleteArvisRequest(Guid Id, long? ExpectedVersion) : ICommand;