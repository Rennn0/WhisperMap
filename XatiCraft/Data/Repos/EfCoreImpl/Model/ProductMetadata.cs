using System.ComponentModel.DataAnnotations;

namespace XatiCraft.Data.Repos.EfCoreImpl.Model;

/// <summary>
/// </summary>
public sealed class ProductMetadata
{
    /// <summary>
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// </summary>

    [MaxLength(512)]
    public string OriginalFile { get; init; } = null!;

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string FileKey { get; init; } = null!;

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string Location { get; init; } = null!;

    /// <summary>
    /// </summary>
    public long ProductId { get; init; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// </summary>
    public Product Product { get; init; } = null!;
}