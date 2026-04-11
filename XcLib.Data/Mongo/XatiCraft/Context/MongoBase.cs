using MongoDB.Driver;

namespace XcLib.Data.Mongo.XatiCraft.Context;

/// <summary>
/// </summary>
public class MongoBase<T> : MongoConnector
{
    
    /// <summary>
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="db"></param>
    protected MongoBase(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    protected static IMongoCollection<T> Collection => Database!.GetCollection<T>(typeof(T).Name);
    protected static IClientSession Session => Client!.StartSession();
}