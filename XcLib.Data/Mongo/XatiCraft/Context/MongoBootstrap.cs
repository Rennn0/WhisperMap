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

        Task[] tasks =
        [
            CreateIndexesAsync(Database.GetCollection<AuthorizationInfoDoc>(MongoBase<AuthorizationInfoDoc>.Name)),
            CreateIndexesAsync(Database.GetCollection<PaymentProviderDoc>(MongoBase<PaymentProviderDoc>.Name)),
            SeedDataAsync(Database.GetCollection<PaymentProviderDoc>(MongoBase<PaymentProviderDoc>.Name))
        ];

        await Task.WhenAll(tasks);
        _logger.LogInformation("mongo indexes created");
    }

    /// <inheritdoc />
    public void Run() => throw new NotImplementedException();

    private static Task<string> CreateIndexesAsync(IMongoCollection<AuthorizationInfoDoc> collection)
    {
        IndexKeysDefinition<AuthorizationInfoDoc>? indexKeys = Builders<AuthorizationInfoDoc>.IndexKeys
            .Ascending(x => x.Email)
            .Ascending(x => x.AuthProviderSystemId);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuthorizationInfoDoc>(indexKeys, indexOptions));
    }

    private static Task<string> CreateIndexesAsync(IMongoCollection<PaymentProviderDoc> collection)
    {
        IndexKeysDefinition<PaymentProviderDoc>? indexKeys = Builders<PaymentProviderDoc>.IndexKeys
            .Ascending(x => x.UniqSelector);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<PaymentProviderDoc>(indexKeys, indexOptions));
    }


    private static Task<BulkWriteResult<PaymentProviderDoc>> SeedDataAsync(
        IMongoCollection<PaymentProviderDoc> collection)
    {
        PaymentProviderDoc[] data =
        [
            new PaymentProviderDoc("Flitt", 1) { Enabled = true },
            new PaymentProviderDoc("Google", 2)
            {
                Enabled = true, LogoUrl = "https://pay.google.com/about/static_kcs/images/logos/google-pay-logo.svg"
            },
            new PaymentProviderDoc("Apple", 3)
                { Enabled = true, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/b/b0/Apple_Pay_logo.svg" }
        ];

        IEnumerable<UpdateOneModel<PaymentProviderDoc>> writes = data.Select(d =>
            new UpdateOneModel<PaymentProviderDoc>(
                Builders<PaymentProviderDoc>.Filter.Eq(x => x.UniqSelector, d.UniqSelector),
                Builders<PaymentProviderDoc>.Update
                    .Set(x => x.Name, d.Name)
                    .Set(x => x.Enabled, d.Enabled)
                    .Set(x => x.Description, d.Description)
                    .Set(x => x.FullName, d.FullName)
                    .Set(x => x.LogoUrl, d.LogoUrl)
            )
            {
                IsUpsert = true
            });

        return collection.BulkWriteAsync(writes);
    }
}