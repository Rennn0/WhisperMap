using System.ComponentModel.DataAnnotations;

namespace XcLib.Data.Postgres.XatiCraft.Model;

/// <summary>
/// </summary>
public sealed class ProductMetadataModel
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

    public int? Order { get; init; }
    public ushort? InStock { get; init; }

    /// <summary>
    /// </summary>
    public ProductModel ProductModel { get; init; } = null!;
}
