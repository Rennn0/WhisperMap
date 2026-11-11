using ASA.Domain.Abstractions;
using ASA.Domain.Enums;
using ASA.Domain.ValueObjects;
using Marten.Metadata;

namespace ASA.Domain;

public sealed class PopularRequestOption : EntityLong, ITracked
{
    public PopularRequestOption(string name, string description, KeyValueProperty property, ConfigPlace place, string correlationId, string causationId, string lastModifiedBy)
    {
        if (!Enum.IsDefined(place))
            throw new ArgumentOutOfRangeException(nameof(place));
        Name = name;
        Description = description;
        Property = property;
        Place = place;
        CorrelationId = correlationId;
        CausationId = causationId;
        LastModifiedBy = lastModifiedBy;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public KeyValueProperty Property { get; set; }
    public ConfigPlace Place { get; set; }
    public string CorrelationId { get; set; }
    public string CausationId { get; set; }
    public string LastModifiedBy { get; set; }
}