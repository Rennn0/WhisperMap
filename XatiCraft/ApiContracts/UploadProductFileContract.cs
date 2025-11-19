using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record UploadProductFileContract(
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}