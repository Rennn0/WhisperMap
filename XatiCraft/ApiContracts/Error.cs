namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Code"></param>
/// <param name="Reason"></param>
/// <param name="Hint"></param>
public record Error(ErrorCode Code, string? Reason = null, string? Hint = null) : ApiContract;