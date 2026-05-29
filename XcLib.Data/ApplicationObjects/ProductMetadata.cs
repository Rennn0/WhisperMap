namespace XcLib.Data.ApplicationObjects;

/// <summary>
/// </summary>
/// <param name="OriginalFile"></param>
/// <param name="FileKey"></param>
/// <param name="Location"></param>
/// <param name="ProductId"></param>
public record ProductMetadata(
    string OriginalFile,
    string FileKey,
    string Location,
    long ProductId,
    int? Order) : ApplicationObject
{
    public ProductMetadata() : this("", "", "", 0, null)
    {
    }
    
    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// </summary>
    public Product? Product { get; set; }

    public static ProductMetadata From(Postgres.XatiCraft.Model.ProductMetadata model)
        => new ProductMetadata(model.OriginalFile, model.FileKey, model.Location, model.ProductId, model.Order)
        {
            Id =  model.Id,
            Timestamp = model.Timestamp,
            Product = model.Product == null! ? null : Product.From(model.Product)
        };
}