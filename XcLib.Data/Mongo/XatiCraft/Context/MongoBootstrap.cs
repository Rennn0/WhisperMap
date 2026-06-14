using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
            SeedDataAsync(Database.GetCollection<AuthorizationInfoDoc>(MongoBase<AuthorizationInfoDoc>.Name)),
            CreateIndexesAsync(Database.GetCollection<PaymentProviderDoc>(MongoBase<PaymentProviderDoc>.Name)),
            SeedDataAsync(Database.GetCollection<PaymentProviderDoc>(MongoBase<PaymentProviderDoc>.Name)),
            SeedDataAsync(Database.GetCollection<ProductCartDoc>(MongoBase<ProductCartDoc>.Name))
        ];

        await Task.WhenAll(tasks);
        _logger.LogInformation("mongo boot finished");
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
                    .SetOnInsert(x => x.Name, d.Name)
                    .SetOnInsert(x => x.Enabled, d.Enabled)
                    .SetOnInsert(x => x.Description, d.Description)
                    .SetOnInsert(x => x.FullName, d.FullName)
                    .SetOnInsert(x => x.LogoUrl, d.LogoUrl)
            )
            {
                IsUpsert = true
            });

        return collection.BulkWriteAsync(writes);
    }

    private static Task<BulkWriteResult<AuthorizationInfoDoc>> SeedDataAsync(
        IMongoCollection<AuthorizationInfoDoc> collection)
    {
        AuthorizationInfoDoc[] data =
        [
            new AuthorizationInfoDoc
            {
                Id = new ObjectId("b7dc9fb35e998892e77fdccf"),
                Username = "dev",
                Email = "dev@xati.org",
                AccountEnabled = true,
                VerifiedEmail = true,
                Created = DateTimeOffset.Parse("6/12/2025 12:20:00 PM +04:00"),
                ProfilePicture =
                    "https://randompicturegenerator.com/img/picture-generator/siyan-ren-qLiFcanSpuA-unsplash.jpg"
            }
        ];

        IEnumerable<UpdateOneModel<AuthorizationInfoDoc>> writes = data.Select(d =>
            new UpdateOneModel<AuthorizationInfoDoc>(
                Builders<AuthorizationInfoDoc>.Filter.Eq(x => x.Id, d.Id),
                Builders<AuthorizationInfoDoc>.Update
                    .Set(x => x.AccountEnabled, d.AccountEnabled)
                    .SetOnInsert(x => x.Created, d.Created)
                    .Set(x => x.ProfilePicture, d.ProfilePicture)
                    .Set(x => x.Email, d.Email)
                    .Set(x => x.Username, d.Username)
                    .Set(x => x.VerifiedEmail, d.VerifiedEmail)
                    .Set(x => x.Id, d.Id)
            )
            {
                IsUpsert = true
            });

        return collection.BulkWriteAsync(writes);
    }

    private static Task<BulkWriteResult<ProductCartDoc>> SeedDataAsync(
        IMongoCollection<ProductCartDoc> collection)
    {
        ProductCartDoc[] data =
        [
            new ProductCartDoc
            {
                Id = new ObjectId("6a2a9e5227e9caea6921f3ee"),
                UserId = "b7dc9fb35e998892e77fdccf",
                ProductIds = ["-10"],
                ProductIdOrderId = []
            }
        ];

        IEnumerable<UpdateOneModel<ProductCartDoc>> writes = data.Select(d =>
            new UpdateOneModel<ProductCartDoc>(
                Builders<ProductCartDoc>.Filter.Eq(x => x.Id, d.Id),
                Builders<ProductCartDoc>.Update
                    .SetOnInsert(x => x.ProductIds, d.ProductIds)
                    .SetOnInsert(x => x.ProductIdOrderId, d.ProductIdOrderId)
                    .Set(x => x.UserId, d.UserId)
                    .Set(x => x.Id, d.Id)
            )
            {
                IsUpsert = true
            });

        return collection.BulkWriteAsync(writes);
    }
}