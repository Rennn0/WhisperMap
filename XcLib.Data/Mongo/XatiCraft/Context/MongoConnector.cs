using MongoDB.Driver;

namespace XcLib.Data.Mongo.XatiCraft.Context;

public class MongoConnector
{
    private static Lazy<MongoClient>? _client;
    private static Lazy<IMongoDatabase>? _database;
    protected static MongoClient? Client => _client?.Value;
    protected static IMongoDatabase? Database => _database?.Value;

    protected MongoConnector(string connection, string db)
    {
        _client ??= new Lazy<MongoClient>(() => new MongoClient(connection),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _database ??= new Lazy<IMongoDatabase>(() => _client.Value.GetDatabase(db),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}