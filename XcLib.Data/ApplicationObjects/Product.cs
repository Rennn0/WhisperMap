namespace XcLib.Data.ApplicationObjects;

/// <summary>
/// </summary>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
public record Product(
    string Title,
    string Description,
    decimal Price
) : ApplicationObject
{
    public Product() : this("", "", 0)
    {
    }
    /// <summary>
    /// </summary>
    public ICollection<ProductMetadata>? ProductMetadata { get; init; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// </summary>
    public string? PreviewImg { get; set; }

    public static Product From(Postgres.XatiCraft.Model.Product model) =>
        new Product(model.Title, model.Description, model.Price)
        {
            Id = model.Id,
            Timestamp = model.Timestamp,
        };
}