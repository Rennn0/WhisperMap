using ASA.Application.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Application.ArvisRequests.Commands;

public sealed record CreateArvisRequest(
    Guid Id,
    string Title,
    IReadOnlyList<KeyValueProperty> Headers,
    IReadOnlyList<KeyValueProperty> Cookies) : ICommand<long>;