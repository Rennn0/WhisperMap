using System.ComponentModel.DataAnnotations;

namespace XcLib.Data.Postgres.XatiCraft.Model;

/// <summary>
/// </summary>
public sealed class ProductModel
{
    /// <summary>
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string Title { get; init; } = null!;

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string Description { get; init; } = null!;

    /// <summary>
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// </summary>
    public ICollection<ProductMetadataModel> ProductMetadata { get; init; } = new List<ProductMetadataModel>();
}