using XcLib.Data.Abstractions;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record GetProductsContext(
    string? Query = null,
    string? ContinuationToken = null,
    uint? Batch = null,
    IEnumerable<long>? Ids = null,
    OrderBy? OrderBy = OrderBy.NewestFirst) : ApiContext;