using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class EntityLong : IEntity<long>
{
    public long Id { get; set; }
}