using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using PaymentProvider = XcLib.Data.Mongo.XatiCraft.Model.PaymentProvider;

namespace XcLib.Data.Mongo.XatiCraft;

public class PaymentProviderRepo : MongoBase<PaymentProvider>, IPaymentProviderRepo
{
    public PaymentProviderRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public Task<ApplicationObjects.PaymentProvider> GetByIdAsync(long id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<ApplicationObjects.PaymentProvider> AddAsync(ApplicationObjects.PaymentProvider obj,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<List<ApplicationObjects.PaymentProvider>> GetAsync(ApplicationObjects.PaymentProvider obj,
        sbyte searchFlag = 0, CancellationToken token = default)
    {
        FilterDefinition<PaymentProvider> filter = ToSearchPredicate(obj, searchFlag);
        return Task.FromResult(
            Collection.Find(filter).ToList(token).Select(ApplicationObjects.PaymentProvider.From).ToList());
    }

    public Task<ApplicationObjects.PaymentProvider?> UpdateAsync(ApplicationObjects.PaymentProvider obj,
        sbyte searchFlag = 0, CancellationToken token = default) => throw new NotImplementedException();

    public Task<int> DeleteAsync(ApplicationObjects.PaymentProvider obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<bool> ExistsAsync(ApplicationObjects.PaymentProvider obj, sbyte searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    protected override FilterDefinition<PaymentProvider> ToSearchPredicate(ApplicationObject applicationObject,
        sbyte searchFlag)
    {
        if (applicationObject is not ApplicationObjects.PaymentProvider paymentProvider)
            throw new ArgumentOutOfRangeException(nameof(applicationObject));

        return searchFlag switch
        {
            sbyte.MaxValue => Builders<PaymentProvider>.Filter.Empty,
            sbyte.MinValue => Builders<PaymentProvider>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId)),
            1 => Builders<PaymentProvider>.Filter.Eq(x => x.UniqSelector, paymentProvider.UniqSelector),
            2 => Builders<PaymentProvider>.Filter.Eq(x => x.Name, paymentProvider.Name),
            _ => Builders<PaymentProvider>.Filter.Eq(x => x.Id, ObjectId.Parse(paymentProvider.ObjId))
        };
    }
}