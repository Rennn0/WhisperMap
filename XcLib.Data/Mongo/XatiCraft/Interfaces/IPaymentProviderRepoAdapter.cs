using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft.Interfaces;

public interface IPaymentProviderRepoAdapter : IPaymentProviderRepo
{
    FilterDefinition<PaymentProviderDoc> ToSearchPredicate(ApplicationObject applicationObject, sbyte searchFlag);
}