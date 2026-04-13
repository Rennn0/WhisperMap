using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;

namespace XcLib.Data.Mongo.XatiCraft.Context;

public class MongoConnector
{
    private static Lazy<MongoClient>? _client;
    private static Lazy<IMongoDatabase>? _database;
    protected static MongoClient? Client => _client?.Value;
    protected static IMongoDatabase? Database => _database?.Value;

    protected MongoConnector(IOptions<MongoConnectionOptions> connectionOptions)
    {
        ConnectionOptions options = connectionOptions.Value;
        ArgumentNullException.ThrowIfNull(options.ConnectionString);
        ArgumentNullException.ThrowIfNull(options.Database);

        _client ??= new Lazy<MongoClient>(() =>
                new MongoClient(options.ConnectionString),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _database ??= new Lazy<IMongoDatabase>(() => _client.Value.GetDatabase(options.Database),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}