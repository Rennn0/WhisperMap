using System.Text.Json.Serialization;
using XcLib.Data.ApplicationObjects;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Products"></param>
/// /// <param name="Context"></param>
internal record GetProductsContract(
    IEnumerable<Product> Products,
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;

    public string? ContinuationToken { get; init; }
}