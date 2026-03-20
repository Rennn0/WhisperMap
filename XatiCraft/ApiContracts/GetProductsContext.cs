namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record GetProductsContext(
    string? Query = null,
    IEnumerable<long>? Ids = null) : ApiContext;