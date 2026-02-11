using MongoDB.Driver;

namespace XatiCraft.Data.Repos.MongoImpl;

/// <summary>
/// </summary>
internal class MongoBase<T> : MongoConnector
{
    
    /// <summary>
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="db"></param>
    protected MongoBase(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    protected static IMongoCollection<T> Collection => Database!.Value.GetCollection<T>(typeof(T).Name);
    protected static IClientSession Session => Client!.Value.StartSession();
}