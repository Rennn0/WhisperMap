namespace XcLib.Data.ApplicationObjects;

public record PageVisitor(string Page, string? IpAddress, string? Uid, string? Browser) : ApplicationObject;