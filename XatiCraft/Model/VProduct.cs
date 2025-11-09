using System.Text.Json;

namespace XatiCraft.Model;

public class VProduct
{
    public long? Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public DateTime? Timestamp { get; set; }

    public JsonDocument? Metadata { get; set; }
}