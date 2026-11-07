using ASA.Application.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Application.ArvisRequests.Commands;

public sealed record CreateArvisRequest(
    string Title,
    IReadOnlyList<KeyValueProperty> Headers,
    IReadOnlyList<KeyValueProperty> Cookies) : ICommand<long>
{
    public Guid Id { get; } = Guid.NewGuid();
}