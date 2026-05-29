namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record UpdateProductContext(long Id, string Title, string Description, decimal Price)
    : CreateProductContext(Title, Description, Price)
{
    /// <summary>
    /// </summary>
    public List<uint>? ExistingResourceIds { get; init; }
    /// <summary>
    /// </summary>
    public List<uint>? ResourceIdsTobeDelete { get; init; }
}