using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class Entity : IEntity
{
    public Guid Id { get; protected init; }
}