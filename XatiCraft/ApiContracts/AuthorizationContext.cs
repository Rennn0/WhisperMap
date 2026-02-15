namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record AuthorizationContext(string Token, ApplicationAuthProvider Provider) : ApiContext;