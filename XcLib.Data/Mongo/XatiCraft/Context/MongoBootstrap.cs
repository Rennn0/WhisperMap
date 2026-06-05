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
            CreateIndexesAsync(Database.GetCollection<AuthorizationInfo>(nameof(AuthorizationInfo))),
            CreateIndexesAsync(Database.GetCollection<PaymentProvider>(nameof(PaymentProvider))),
            SeedDataAsync(Database.GetCollection<PaymentProvider>(nameof(PaymentProvider)))
        ];

        await Task.WhenAll(tasks);
        _logger.LogInformation("mongo indexes created");
    }

    /// <inheritdoc />
    public void Run() => throw new NotImplementedException();

    private static Task<string> CreateIndexesAsync(IMongoCollection<AuthorizationInfo> collection)
    {
        IndexKeysDefinition<AuthorizationInfo>? indexKeys = Builders<AuthorizationInfo>.IndexKeys
            .Ascending(x => x.Email)
            .Ascending(x => x.AuthProviderSystemId);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<AuthorizationInfo>(indexKeys, indexOptions));
    }

    private static Task<string> CreateIndexesAsync(IMongoCollection<PaymentProvider> collection)
    {
        IndexKeysDefinition<PaymentProvider>? indexKeys = Builders<PaymentProvider>.IndexKeys
            .Ascending(x => x.UniqSelector);

        CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };

        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<PaymentProvider>(indexKeys, indexOptions));
    }


    private static Task<BulkWriteResult<PaymentProvider>> SeedDataAsync(IMongoCollection<PaymentProvider> collection)
    {
        PaymentProvider[] data =
        [
            new PaymentProvider("Flitt", 1) { Enabled = true },
            new PaymentProvider("Google", 2)
            {
                Enabled = true, LogoUrl = "https://pay.google.com/about/static_kcs/images/logos/google-pay-logo.svg"
            },
            new PaymentProvider("Apple", 3)
                { Enabled = true, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/b/b0/Apple_Pay_logo.svg" }
        ];

        IEnumerable<UpdateOneModel<PaymentProvider>> writes = data.Select(d =>
            new UpdateOneModel<PaymentProvider>(
                Builders<PaymentProvider>.Filter.Eq(x => x.UniqSelector, d.UniqSelector),
                Builders<PaymentProvider>.Update
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