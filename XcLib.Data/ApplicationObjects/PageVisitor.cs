namespace XcLib.Data.ApplicationObjects;

public record PageVisitor(string Page, string? IpAddress, string? Uid, string? Browser) : ApplicationObject
{
    public PageVisitor() : this("", null, null, null)
    {
    }

    public static PageVisitor From(Postgres.XatiCraft.Model.PageVisitor model) =>
        new PageVisitor(model.Page, model.IpAddress, model.Uid, model.Browser)
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt
        };
}