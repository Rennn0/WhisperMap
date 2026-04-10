using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Realtime.Entities;

[Table("RealtimeCache", Schema = "cache")]
[Index("ExpiresAtTime", Name = "Index_ExpiresAtTime")]
public class RealtimeCache
{
    [Key] [StringLength(449)] public string Id { get; set; } = null!;

    public byte[] Value { get; set; } = null!;

    public DateTimeOffset ExpiresAtTime { get; set; }

    public long? SlidingExpirationInSeconds { get; set; }

    public DateTimeOffset? AbsoluteExpiration { get; set; }
}