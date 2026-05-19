namespace XcLib.Data.ApplicationObjects;

public record PageVisitor(string Page, string? IpAddress, string? Uid, string? Browser) : ApplicationObject
{
    public static PageVisitor From(Postgres.XatiCraft.Model.PageVisitor visitor) =>
        new PageVisitor(visitor.Page, visitor.IpAddress, visitor.Uid, visitor.Browser)
        {
            Id = visitor.Id,
            CreatedAt = visitor.CreatedAt
        };
}