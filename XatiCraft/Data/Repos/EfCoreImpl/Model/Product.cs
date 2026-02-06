using System.ComponentModel.DataAnnotations;

namespace XatiCraft.Data.Repos.EfCoreImpl.Model;

/// <summary>
/// </summary>
public sealed class Product
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
    public ICollection<ProductMetadata> ProductMetadata { get; init; } = new List<ProductMetadata>();
}