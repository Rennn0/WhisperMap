using System;
using System.Collections.Generic;

namespace XatiCraft.Model;

public partial class ProductMetadata
{
    public long Id { get; set; }

    public string OriginalFile { get; set; } = null!;

    public string FileKey { get; set; } = null!;

    public string Location { get; set; } = null!;

    public long ProductId { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Product Product { get; set; } = null!;
}
