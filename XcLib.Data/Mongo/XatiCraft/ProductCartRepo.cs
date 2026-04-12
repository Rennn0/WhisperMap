using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class ProductCartRepo : MongoBase<ProductCart>, IProductCartRepo
{
    public ProductCartRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public async ValueTask<ApplicationObjects.ProductCart> UpsertAsync(ApplicationObjects.ProductCart productCart,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCart> filterDefinition = Builders<ProductCart>.Filter.And(
            Builders<ProductCart>.Filter.Eq(x => x.UserId, productCart.UserId)
        );

        UpdateDefinition<ProductCart> updateDefinition =
            Builders<ProductCart>.Update
                .SetOnInsert(x => x.UserId, productCart.UserId)
                .AddToSetEach(x => x.ProductIds, productCart.ProductIds);

        FindOneAndUpdateOptions<ProductCart> options = new FindOneAndUpdateOptions<ProductCart>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        ProductCart? model =
            await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, options, cancellationToken);

        ArgumentNullException.ThrowIfNull(model);
        
        return productCart with
        {
            ObjId = model.Id
        };
    }

    public async ValueTask<ApplicationObjects.ProductCart?> SelectAsync(string userId,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCart> filterDefinition = Builders<ProductCart>.Filter.Eq(x => x.UserId, userId);
        ProductCart? model = await Collection.Find(filterDefinition).FirstOrDefaultAsync(cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new ApplicationObjects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }

    public async ValueTask<ApplicationObjects.ProductCart?> RemoveAsync(string userId, string productId,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCart> filterDefinition = Builders<ProductCart>.Filter.Eq(x => x.UserId, userId);
        UpdateDefinition<ProductCart>
            updateDefinition = Builders<ProductCart>.Update.Pull(x => x.ProductIds, productId);
        FindOneAndUpdateOptions<ProductCart> updateOptions = new FindOneAndUpdateOptions<ProductCart>
        {
            ReturnDocument = ReturnDocument.After
        };
        ProductCart? model = await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, updateOptions,
            cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new ApplicationObjects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }

    public async ValueTask<ApplicationObjects.ProductCart?> RemoveAsync(string userId, IEnumerable<string> productIds,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCart> filterDefinition = Builders<ProductCart>.Filter.Eq(x => x.UserId, userId);
        UpdateDefinition<ProductCart>
            updateDefinition = Builders<ProductCart>.Update.PullAll(x => x.ProductIds, productIds);
        FindOneAndUpdateOptions<ProductCart> updateOptions = new FindOneAndUpdateOptions<ProductCart>
        {
            ReturnDocument = ReturnDocument.After
        };
        ProductCart? model = await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, updateOptions,
            cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new ApplicationObjects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }
}