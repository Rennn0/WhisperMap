using System.ComponentModel.DataAnnotations;

namespace XcLib.Data.SqlServer.Realtime.Entities;

public class Log
{
    [Key] public int Id { get; set; }

    public int EventId { get; set; }

    public int Level { get; set; }

    public string? Message { get; set; }

    [StringLength(255)] public string? Name { get; set; }

    public DateTimeOffset TimeStamp { get; set; }
}