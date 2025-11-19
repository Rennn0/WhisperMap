using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record CreateProductContract(long ProductId, ApiContext Context) : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}