using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace XatiCraft.Data.Repos.EfCoreImpl.Model;

/// <summary>
/// </summary>
public class VProduct
{
    /// <summary>
    /// </summary>
    public long? Id { get; init; }

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string? Title { get; init; }

    /// <summary>
    /// </summary>
    [MaxLength(512)]
    public string? Description { get; init; }

    /// <summary>
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// </summary>
    public JsonDocument? Metadata { get; init; }
}