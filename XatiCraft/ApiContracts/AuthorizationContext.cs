namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AuthorizationContext(string Token, string Provider) : ApiContext;