using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.ApplicationObjects;

public record PaymentProvider(string Name, sbyte UniqSelector) : ApplicationObject
{
    public string? LogoUrl { get; init; }
    public string? FullName { get; init; }
    public string? Description { get; init; }
    public bool? Enabled { get; init; }

    public PaymentProvider() : this(string.Empty, 0)
    {
    }

    public static PaymentProvider From(PaymentProviderDoc model) =>
        new PaymentProvider(model.Name, model.UniqSelector)
        {
            LogoUrl = model.LogoUrl,
            FullName = model.FullName,
            Description = model.Description,
            Enabled = model.Enabled,
            ObjId = model.Id.ToString()
        };
}