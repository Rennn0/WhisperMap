using System.Text.Json.Serialization;
using XatiCraft.Data.Objects;

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
}