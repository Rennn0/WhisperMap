using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AuthorizationContract(string Uid, string Username, string? ProfilePicture, ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}