using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Interfaces;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class PaymentProviderRepoAdapter :
    MongoBase<PaymentProviderDoc>,
    IPaymentProviderRepoAdapter
{
    public PaymentProviderRepoAdapter(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public Task<PaymentProvider> GetByIdAsync(long id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<PaymentProvider> AddAsync(PaymentProvider obj,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<List<PaymentProvider>> GetAsync(PaymentProvider obj,
        sbyte searchFlag = 0, CancellationToken token = default)
    {
        FilterDefinition<PaymentProviderDoc> filter = ToSearchPredicate(obj, searchFlag);
        return Task.FromResult(
            Collection.Find(filter).ToList(token).Select(PaymentProvider.From).ToList());
    }

    public Task<PaymentProvider?> UpdateAsync(PaymentProvider obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(PaymentProvider obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(PaymentProvider obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public FilterDefinition<PaymentProviderDoc> ToSearchPredicate(ApplicationObject applicationObject,
        sbyte searchFlag) 
    {
        if (applicationObject is not PaymentProvider paymentProvider)
            throw new ArgumentOutOfRangeException(nameof(applicationObject));
    
        return searchFlag switch
        {
            sbyte.MaxValue => Builders<PaymentProviderDoc>.Filter.Empty,
            sbyte.MinValue => Builders<PaymentProviderDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId)),
            1 => Builders<PaymentProviderDoc>.Filter.Eq(x => x.UniqSelector, paymentProvider.UniqSelector),
            2 => Builders<PaymentProviderDoc>.Filter.Eq(x => x.Name, paymentProvider.Name),
            _ => Builders<PaymentProviderDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId))
        };
    }
}