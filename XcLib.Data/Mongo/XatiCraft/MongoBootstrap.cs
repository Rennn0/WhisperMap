using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

/// <inheritdoc />
public class MongoBootstrap : IBootstrap
{
    private readonly string _connection;
    private readonly string _databaseName;

    public MongoBootstrap(string connection, string databaseName = "xc-db")
    {
        _connection = connection;
        _databaseName = databaseName;
    }

    /// <inheritdoc />
    public async ValueTask RunAsync()
    {
        MongoClient client = new MongoClient(_connection);
        IMongoDatabase? database = client.GetDatabase(_databaseName);

        IMongoCollection<AuthorizationInfo>?
            collection = database.GetCollection<AuthorizationInfo>(nameof(AuthorizationInfo));

        IndexKeysDefinition<AuthorizationInfo>? indexKeys = Builders<AuthorizationInfo>.IndexKeys
            .Ascending(x => x.Email)
            .Ascending(x => x.AuthProviderSystemId);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuthorizationInfo>(indexKeys, indexOptions));
    }

    /// <inheritdoc />
    public void Run() => RunAsync().GetAwaiter().GetResult();
}