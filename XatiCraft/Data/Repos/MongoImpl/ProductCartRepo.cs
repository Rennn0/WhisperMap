using MongoDB.Driver;
using XatiCraft.Data.Repos.MongoImpl.Model;

namespace XatiCraft.Data.Repos.MongoImpl;

internal class ProductCartRepo : MongoBase<ProductCart>, IProductCartRepo
{
    internal ProductCartRepo(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    public async ValueTask<Objects.ProductCart> UpsertAsync(Objects.ProductCart productCart,
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

    public async ValueTask<Objects.ProductCart?> SelectAsync(string userId, CancellationToken cancellationToken)
    {
        FilterDefinition<ProductCart> filterDefinition = Builders<ProductCart>.Filter.Eq(x => x.UserId, userId);
        ProductCart? model = await Collection.Find(filterDefinition).FirstOrDefaultAsync(cancellationToken);
        return model is not { UserId: not null }
            ? null
            : new Objects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }

    public async ValueTask<Objects.ProductCart?> RemoveAsync(string userId, string productId,
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
            : new Objects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }

    public async ValueTask<Objects.ProductCart?> RemoveAsync(string userId, IEnumerable<string> productIds,
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
            : new Objects.ProductCart(model.UserId) { ProductIds = model.ProductIds };
    }
}