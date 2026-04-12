using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace XcLib.Data.SqlServer.Realtime.Entities;

[Table("Log", Schema = "realtime")]
public class RealtimeLog : Log<int> 
{
    [Key] public override int Id { get; set; }

    public override int EventId { get; set; }

    public override int Level { get; set; }

    public override string? Message { get; set; }

    [StringLength(255)] public override string? Name { get; set; }

    public override DateTimeOffset TimeStamp { get; set; }
}