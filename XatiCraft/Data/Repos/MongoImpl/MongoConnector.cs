using MongoDB.Driver;

namespace XatiCraft.Data.Repos.MongoImpl;

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