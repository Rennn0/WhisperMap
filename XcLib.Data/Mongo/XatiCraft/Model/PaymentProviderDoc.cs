using System.ComponentModel;

namespace XcLib.Data.Mongo.XatiCraft.Model;

[Description("payment_provider")]
public record PaymentProviderDoc(string Name, sbyte UniqSelector) : MongoDoc
{
    public string? LogoUrl { get; init; }
    public string? FullName { get; init; }
    public string? Description { get; init; }
    public bool? Enabled { get; init; }
}