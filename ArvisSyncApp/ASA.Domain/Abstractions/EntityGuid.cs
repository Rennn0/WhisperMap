using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class EntityGuid : IEntity<Guid>
{
    public Guid Id { get; protected set; }
}