using PageVisitor = XcLib.Data.Postgres.XatiCraft.Model.PageVisitor;

namespace XcLib.Data.Abstractions;

public interface IPageVisitorRepo : IBasicRepo<PageVisitor, ApplicationObjects.PageVisitor>
{
}