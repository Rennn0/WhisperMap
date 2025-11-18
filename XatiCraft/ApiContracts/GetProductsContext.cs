namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record GetProductsContext(
    string? Query = null,
    bool? FromCookies = null,
    Dictionary<string, string>? Cookies = null) : ApiContext;