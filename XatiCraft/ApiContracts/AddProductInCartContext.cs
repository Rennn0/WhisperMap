namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AddProductInCartContext(long ProductId, Dictionary<string, string> ExistingCookies) : ApiContext;