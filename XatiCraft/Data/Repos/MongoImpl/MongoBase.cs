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

    protected IMongoCollection<T> Collection => Database!.Value.GetCollection<T>(typeof(T).Name);
    protected IClientSession Session => Client!.Value.StartSession();
}

internal class MongoConnector
{
    protected static Lazy<MongoClient>? Client;
    protected static Lazy<IMongoDatabase>? Database;

    protected MongoConnector(string connection, string db)
    {
        Client ??= new Lazy<MongoClient>(() => new MongoClient(connection),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Database ??= new Lazy<IMongoDatabase>(() => Client.Value.GetDatabase(db),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}