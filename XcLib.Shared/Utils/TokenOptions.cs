namespace XcLib.Shared.Utils;

public class TokenOptions
{
    public int? Expires { get; set; }
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}