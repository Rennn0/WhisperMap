using ASA.Application.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Application.ArvisRequests.Commands;

public sealed record UpdateArvisRequest(
    Guid Id,
    long ExpectedVersion,
    string Title,
    IReadOnlyList<KeyValueProperty> Headers,
    IReadOnlyList<KeyValueProperty> Cookies) : ICommand<long>;