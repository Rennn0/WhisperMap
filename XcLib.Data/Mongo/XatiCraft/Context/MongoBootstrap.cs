using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft.Context;

public class MongoBootstrap : MongoConnector, IBootstrap
{
    private readonly ILogger<MongoBootstrap> _logger;

    public MongoBootstrap(IOptions<MongoConnectionOptions> connectionOptions, ILogger<MongoBootstrap> logger) : base(
        connectionOptions) =>
        _logger = logger;

    /// <inheritdoc />
    public async ValueTask RunAsync()
    {
        ArgumentNullException.ThrowIfNull(Database);

        IMongoCollection<AuthorizationInfo>
            collection = Database.GetCollection<AuthorizationInfo>(nameof(AuthorizationInfo));

        IndexKeysDefinition<AuthorizationInfo>? indexKeys = Builders<AuthorizationInfo>.IndexKeys
            .Ascending(x => x.Email)
            .Ascending(x => x.AuthProviderSystemId);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuthorizationInfo>(indexKeys, indexOptions));

        _logger.LogInformation("mongo indexes created");
    }

    /// <inheritdoc />
    public void Run() => throw new NotImplementedException();
}