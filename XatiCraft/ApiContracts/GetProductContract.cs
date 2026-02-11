using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Id"></param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="InCart"></param>
/// <param name="Price"></param>
/// <param name="Resources"></param>
/// <param name="Context"></param>
public record GetProductContract(
    string Id,
    string Title,
    string Description,
    decimal Price,
    bool InCart,
    IEnumerable<string>? Resources,
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}