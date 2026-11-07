using ASA.Domain.ValueObjects;

namespace ASA.Application.ArvisRequests;

public sealed class ArvisRequestView
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<KeyValueProperty> Headers { get; set; } = [];
    public List<KeyValueProperty> Cookies { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}