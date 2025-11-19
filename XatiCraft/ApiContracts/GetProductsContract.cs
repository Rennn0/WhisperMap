using System.Text.Json.Serialization;
using XatiCraft.Objects;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Products"></param>
public record GetProductsContract(
    IEnumerable<Product> Products,
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}