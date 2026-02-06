using MongoDB.Driver;

namespace XatiCraft.Data.Repos.MongoImpl;

/// <summary>
/// </summary>
internal class MongoBase<T>
{
    private readonly IMongoDatabase _db;

    /// <summary>
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="db"></param>
    protected MongoBase(string connection, string db = "xc-db")
    {
        MongoClient client = new(connection);
        _db = client.GetDatabase(db);
    }

    protected IMongoCollection<T> GetCollection => _db.GetCollection<T>(typeof(T).Name);
}