namespace XcLib.Shared.Utils;

public class TokenOptions
{
    public int? ExpireSec { get; init; }
    public string? Key { get; init; }
    public string? Issuer { get; init; }
    public string? Audience { get; init; }
}