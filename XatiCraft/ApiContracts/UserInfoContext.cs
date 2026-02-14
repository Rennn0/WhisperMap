namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record UserInfoContext(string Id, string? Provider = null) : ApiContext;