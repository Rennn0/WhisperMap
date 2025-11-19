using System.Text.Json.Serialization;

namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AddProductInCartContract(
    bool AsCookie,
    string ProtectedCookie,
    string PlainCookie,
    string CookieKey,
    ApiContext Context)
    : ApiContract(Context)
{
    /// <summary>
    /// </summary>
    [JsonIgnore]
    public ApiContext Context { get; init; } = Context;
}