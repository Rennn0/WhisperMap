using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft.Context;

/// <summary>
/// </summary>
public class MongoBase<TDoc> : MongoConnector where TDoc : MongoDoc
{
    protected MongoBase(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    protected IMongoCollection<TDoc> Collection => Database!.GetCollection<TDoc>(Name);
    protected IClientSession Session => Client!.StartSession();

    public static string Name =>
        typeof(TDoc).GetCustomAttribute<DescriptionAttribute>()?.Description ?? typeof(TDoc).Name;
}