using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class ProductCartRepo : MongoBase<ProductCartDoc>, IProductCartRepo
{
    public ProductCartRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public async ValueTask<ProductCart> UpsertAsync(ProductCart productCart,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCartDoc> filterDefinition = Builders<ProductCartDoc>.Filter.And(
            Builders<ProductCartDoc>.Filter.Eq(x => x.UserId, productCart.UserId)
        );

        UpdateDefinition<ProductCartDoc> updateDefinition =
            Builders<ProductCartDoc>.Update
                .SetOnInsert(x => x.UserId, productCart.UserId)
                .AddToSetEach(x => x.ProductIds, productCart.ProductIds ?? [])
                .AddToSetEach(x => x.ProductIdOrderId, productCart.ProductIdOrderId ?? []);

        FindOneAndUpdateOptions<ProductCartDoc> options = new FindOneAndUpdateOptions<ProductCartDoc>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        ProductCartDoc? model =
            await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, options, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(model);
        
        return productCart with
        {
            ObjId = model.Id.ToString()
        };
    }

    public async ValueTask<ProductCart?> SelectAsync(string userId,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCartDoc> filterDefinition = Builders<ProductCartDoc>.Filter.Eq(x => x.UserId, userId);
        ProductCartDoc? model = await Collection.Find(filterDefinition).FirstOrDefaultAsync(cancellationToken);
        return model is not { UserId: not null }
            ? null
            : ProductCart.From(model);
    }

    public async ValueTask<ProductCart?> RemoveAsync(string userId, string productId,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCartDoc> filterDefinition = Builders<ProductCartDoc>.Filter.Eq(x => x.UserId, userId);
        UpdateDefinition<ProductCartDoc>
            updateDefinition = Builders<ProductCartDoc>.Update.Pull(x => x.ProductIds, productId);
        FindOneAndUpdateOptions<ProductCartDoc> updateOptions = new FindOneAndUpdateOptions<ProductCartDoc>
        {
            ReturnDocument = ReturnDocument.After
        };
        ProductCartDoc? model = await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition,
            updateOptions,
            cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }

    public async ValueTask<ProductCart?> RemoveAsync(string userId, IEnumerable<string> productIds,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCartDoc> filterDefinition = Builders<ProductCartDoc>.Filter.Eq(x => x.UserId, userId);
        UpdateDefinition<ProductCartDoc>
            updateDefinition = Builders<ProductCartDoc>.Update.PullAll(x => x.ProductIds, productIds);
        FindOneAndUpdateOptions<ProductCartDoc> updateOptions = new FindOneAndUpdateOptions<ProductCartDoc>
        {
            ReturnDocument = ReturnDocument.After
        };
        ProductCartDoc? model = await Collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition,
            updateOptions,
            cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }
}