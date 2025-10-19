using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhisperMap.Models;

[Table("object")]
[Index("Pinid", Name = "object_pinid_key", IsUnique = true)]
public partial class Object
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("pinid")]
    public int? Pinid { get; set; }

    [Column("data")]
    public byte[]? Data { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [ForeignKey("Pinid")]
    [InverseProperty("Object")]
    public virtual Pin? Pin { get; set; }
}
