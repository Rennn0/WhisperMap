using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Url"></param>
public record GetSignedUrlContract(
    string Url,
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}