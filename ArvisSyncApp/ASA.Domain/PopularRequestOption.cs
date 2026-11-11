using ASA.Domain.Abstractions;
using ASA.Domain.Enums;
using ASA.Domain.ValueObjects;

namespace ASA.Domain;

public sealed class PopularRequestOption : EntityLong
{
    public PopularRequestOption(string name, string description, KeyValueProperty property, ConfigPlace place)
    {
        if (!Enum.IsDefined(place))
            throw new ArgumentOutOfRangeException(nameof(place));
        Name = name;
        Description = description;
        Property = property;
        Place = place;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public KeyValueProperty Property { get; set; }
    public ConfigPlace Place { get; set; }
}