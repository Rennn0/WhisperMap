using MongoDB.Driver;
using XatiCraft.Data.Repos.MongoImpl.Model;

namespace XatiCraft.Data.Repos.MongoImpl;

internal class AuthorizationRepo : MongoBase<AuthorizationInfo>, IAuthorizationRepo
{
    public AuthorizationRepo(string connection, string db = "xc-db") : base(connection, db)
    {
    }

    public ValueTask<Objects.AuthorizationInfo?> SelectAuthorizationInfoAsync(long id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<Objects.AuthorizationInfo?> SelectAuthorizationInfoAsync(string id,
        CancellationToken cancellationToken)
    {
        FilterDefinition<AuthorizationInfo>? filter = Builders<AuthorizationInfo>.Filter.Eq(x => x.Id, id);
        AuthorizationInfo? authInfo = await Collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        return authInfo is not { Username.Length: > 0, Created: not null }
            ? null
            : new Objects.AuthorizationInfo(authInfo.Username, authInfo.Created.Value)
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

    public async Task<Objects.AuthorizationInfo> UpsertAuthorizationInfoAsync(
        Objects.AuthorizationInfo authorizationInfo,
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

        AuthorizationInfo? res = await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return authorizationInfo with
        {
            ObjId = res.Id
        };
    }
}