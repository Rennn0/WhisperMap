using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace WhisperMap.Models;

[Table("pin")]
public class Pin
{
    [Key] [Column("id")] public int Id { get; set; }

    [Column("pedestrianid")] public int Pedestrianid { get; set; }

    [Column("location", TypeName = "geography(Point,4326)")]
    public Point Location { get; set; } = null!;

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [InverseProperty("Pin")] public virtual Object? Object { get; set; }

    [ForeignKey("Pedestrianid")]
    [InverseProperty("Pins")]
    public virtual Pedestrian Pedestrian { get; set; } = null!;
}

public class PinWithDistance
{
    public PinWithDistance(Pin pin, double distance)
    {
        Pin = pin;
        Distance = distance;
    }

    public Pin Pin { get; }
    public double Distance { get; }
}