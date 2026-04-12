using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XcLib.Data.Abstractions;
using XcLib.Data.Mongo.XatiCraft.Context;
using XcLib.Data.Mongo.XatiCraft.Model;

namespace XcLib.Data.Mongo.XatiCraft;

public class AuthorizationRepo : MongoBase<AuthorizationInfo>, IAuthorizationRepo
{
    public AuthorizationRepo(IOptions<MongoConnectionOptions> connectionOptions) : base(connectionOptions)
    {
    }

    public ValueTask<ApplicationObjects.AuthorizationInfo?> SelectAsync(long id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<ApplicationObjects.AuthorizationInfo?> SelectAsync(string id,
        CancellationToken cancellationToken)
    {
        FilterDefinition<AuthorizationInfo>? filter = Builders<AuthorizationInfo>.Filter.Eq(x => x.Id, id);
        AuthorizationInfo? authInfo = await Collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        return authInfo is not { Username.Length: > 0, Created: not null }
            ? null
            : new ApplicationObjects.AuthorizationInfo(authInfo.Username, authInfo.Created.Value)
            {
                AccountEnabled = authInfo.AccountEnabled,
                AuthProvider = authInfo.AuthProvider,
                CreationToken = authInfo.CreationToken,
                Email = authInfo.Email,
                ProfilePicture = authInfo.ProfilePicture,
                VerifiedEmail = authInfo.VerifiedEmail,
                ObjId = authInfo.Id
            };
    }

    public async Task<ApplicationObjects.AuthorizationInfo> UpsertAsync(
        ApplicationObjects.AuthorizationInfo authorizationInfo,
        CancellationToken cancellationToken)
    {
        FilterDefinition<AuthorizationInfo>? filter = Builders<AuthorizationInfo>.Filter.And(
            Builders<AuthorizationInfo>.Filter.Eq(x => x.Email, authorizationInfo.Email),
            Builders<AuthorizationInfo>.Filter.Eq(x => x.AuthProviderSystemId, authorizationInfo.AuthProviderSystemId)
        );

        UpdateDefinition<AuthorizationInfo>? update = Builders<AuthorizationInfo>.Update
            .Set(x => x.AccountEnabled, authorizationInfo.AccountEnabled)
            .Set(x => x.VerifiedEmail, authorizationInfo.VerifiedEmail)
            .Set(x => x.Username, authorizationInfo.Username)
            .Set(x => x.AuthProvider, authorizationInfo.AuthProvider)
            .Set(x => x.ProfilePicture, authorizationInfo.ProfilePicture)
            .Set(x => x.AuthProviderOfficial, authorizationInfo.AuthProviderOfficial)
            .Set(x => x.Email, authorizationInfo.Email)
            .SetOnInsert(x => x.CreationToken, authorizationInfo.CreationToken)
            .SetOnInsert(x => x.Created, authorizationInfo.Created);

        FindOneAndUpdateOptions<AuthorizationInfo> options = new FindOneAndUpdateOptions<AuthorizationInfo>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        AuthorizationInfo? model = await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        
        return authorizationInfo with
        {
            ObjId = model.Id
        };
    }
}