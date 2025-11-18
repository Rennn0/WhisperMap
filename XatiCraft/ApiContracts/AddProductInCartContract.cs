namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AddProductInCartContract(bool AsCookie, string ProtectedCookie, string PlainCookie, string CookieKey)
    : ApiContract;