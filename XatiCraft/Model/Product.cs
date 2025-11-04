namespace XatiCraft.Model;

public class Product
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual ICollection<ProductMetadata> ProductMetadata { get; set; } = new List<ProductMetadata>();
}