using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace XcLib.Data.Mongo.XatiCraft.Context;

/// <summary>
/// </summary>
public class MongoBase<T> : MongoConnector
{
    protected MongoBase(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    protected static IMongoCollection<T> Collection => Database!.GetCollection<T>(typeof(T).Name);
    protected static IClientSession Session => Client!.StartSession();
}