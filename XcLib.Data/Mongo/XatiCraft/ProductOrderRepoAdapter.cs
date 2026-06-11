using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Interfaces;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class ProductOrderRepoAdapter : MongoBase<ProductOrderDoc>, IProductOrderRepoAdapter
{
    public ProductOrderRepoAdapter(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public FilterDefinition<ProductOrderDoc> ToSearchPredicate(ApplicationObject applicationObject, sbyte searchFlag)
    {
        if (applicationObject is not ProductOrder paymentProvider)
            throw new ArgumentOutOfRangeException(nameof(applicationObject));

        return searchFlag switch
        {
            sbyte.MaxValue => Builders<ProductOrderDoc>.Filter.Empty,
            sbyte.MinValue => Builders<ProductOrderDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId)),
            1 => Builders<ProductOrderDoc>.Filter.Eq(x => x.OrderOwner, paymentProvider.OrderOwner),
            2 => Builders<ProductOrderDoc>.Filter.Eq(x => x.ProductId, paymentProvider.ProductId),
            3 => Builders<ProductOrderDoc>.Filter.Eq(x => x.PaymentProvider, paymentProvider.PaymentProvider),
            4 => Builders<ProductOrderDoc>.Filter.Eq(x => x.InternalOrderId, paymentProvider.InternalOrderId),
            _ => Builders<ProductOrderDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId))
        };
    }

    public Task<ProductOrder> GetByIdAsync(long id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public async Task<ProductOrder> AddAsync(ProductOrder obj, CancellationToken token = default)
    {
        ProductOrderDoc doc = ProductOrder.From(obj);
        await Collection.InsertOneAsync(doc, new InsertOneOptions { BypassDocumentValidation = true }, token);
        return obj with { ObjId = doc.Id.ToString() };
    }

    public Task<List<ProductOrder>>
        GetAsync(ProductOrder obj, sbyte searchFlag = 0, CancellationToken token = default) =>
        Task.FromResult(
            Collection.Find(ToSearchPredicate(obj, searchFlag)).ToList(token).Select(ProductOrder.From).ToList());

    public async Task<ProductOrder?> UpdateAsync(ProductOrder obj, sbyte searchFlag = 0,
        CancellationToken token = default)
    {
        await Collection.UpdateManyAsync(ToSearchPredicate(obj, searchFlag),
            Builders<ProductOrderDoc>.Update
                .Set(x => x.ProductId, obj.ProductId)
                .Set(x => x.OrderStatus, obj.OrderStatus)
                .Set(x => x.ProviderOrderId, obj.ProviderOrderId),
            cancellationToken: token);

        return null;
    }

    public Task<int> DeleteAsync(ProductOrder obj, sbyte searchFlag = 0, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<bool> ExistsAsync(ProductOrder obj, sbyte searchFlag = 0, CancellationToken token = default) =>
        throw new NotImplementedException();
}