using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;

namespace XcLib.Data.Mongo.XatiCraft.Context;

public class MongoConnector : IDisposable
{
    private readonly Lazy<MongoClient>? _client;
    private readonly Lazy<IMongoDatabase>? _database;
    protected MongoClient? Client => _client?.Value;
    protected IMongoDatabase? Database => _database?.Value;

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

    public void Dispose()
    {
        if (_client is { IsValueCreated: true }) _client.Value.Dispose();
        GC.SuppressFinalize(this);
    }
}