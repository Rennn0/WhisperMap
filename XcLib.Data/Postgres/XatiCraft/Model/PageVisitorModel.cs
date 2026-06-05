using System.ComponentModel.DataAnnotations;

namespace XcLib.Data.Postgres.XatiCraft.Model;

public sealed class PageVisitorModel
{
    public long Id { get; init; }
    [MaxLength(128)] public string Page { get; init; } = null!;
    [MaxLength(64)] public string? IpAddress { get; init; }
    [MaxLength(128)] public string? Uid { get; init; }
    [MaxLength(256)] public string? Browser { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}