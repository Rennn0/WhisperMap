using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class AuthorizationRepoAdapter : MongoBase<AuthorizationInfoDoc>, IAuthorizationRepo
{
    public AuthorizationRepoAdapter(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public ValueTask<AuthorizationInfo?> SelectAsync(long id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<AuthorizationInfo?> SelectAsync(string id,
        CancellationToken cancellationToken)
    {
        FilterDefinition<AuthorizationInfoDoc>? filter =
            Builders<AuthorizationInfoDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(id));
        AuthorizationInfoDoc? authInfo = await Collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        return authInfo is not { Username.Length: > 0, Created: not null }
            ? null
            : new AuthorizationInfo(authInfo.Username, authInfo.Created.Value)
            {
                AccountEnabled = authInfo.AccountEnabled,
                AuthProvider = authInfo.AuthProvider,
                CreationToken = authInfo.CreationToken,
                Email = authInfo.Email,
                ProfilePicture = authInfo.ProfilePicture,
                VerifiedEmail = authInfo.VerifiedEmail,
                ObjId = authInfo.Id.ToString()
            };
    }

    public async Task<AuthorizationInfo> UpsertAsync(
        AuthorizationInfo authorizationInfo,
        CancellationToken cancellationToken)
    {
        FilterDefinition<AuthorizationInfoDoc>? filter = Builders<AuthorizationInfoDoc>.Filter.And(
            Builders<AuthorizationInfoDoc>.Filter.Eq(x => x.Email, authorizationInfo.Email),
            Builders<AuthorizationInfoDoc>.Filter.Eq(x => x.AuthProviderSystemId,
                authorizationInfo.AuthProviderSystemId)
        );

        UpdateDefinition<AuthorizationInfoDoc>? update = Builders<AuthorizationInfoDoc>.Update
            .Set(x => x.AccountEnabled, authorizationInfo.AccountEnabled)
            .Set(x => x.VerifiedEmail, authorizationInfo.VerifiedEmail)
            .Set(x => x.Username, authorizationInfo.Username)
            .Set(x => x.AuthProvider, authorizationInfo.AuthProvider)
            .Set(x => x.ProfilePicture, authorizationInfo.ProfilePicture)
            .Set(x => x.AuthProviderOfficial, authorizationInfo.AuthProviderOfficial)
            .Set(x => x.Email, authorizationInfo.Email)
            .SetOnInsert(x => x.CreationToken, authorizationInfo.CreationToken)
            .SetOnInsert(x => x.Created, authorizationInfo.Created);

        FindOneAndUpdateOptions<AuthorizationInfoDoc> options = new FindOneAndUpdateOptions<AuthorizationInfoDoc>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        AuthorizationInfoDoc? model =
            await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        
        return authorizationInfo with
        {
            ObjId = model.Id.ToString()
        };
    }
}