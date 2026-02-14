using Google.Apis.Auth;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.MongoImpl;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
public class GoogleAuthHandler : IAuthorizationHandler
{
    private readonly IAuthorizationRepo _dbMongo;

    /// <summary>
    /// </summary>
    /// <param name="repos"></param>
    public GoogleAuthHandler(IEnumerable<IAuthorizationRepo> repos)
    {
        _dbMongo = repos.First(r => r is AuthorizationRepo);
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(AuthorizationContext context, CancellationToken cancellationToken)
    {
        GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
        {
            //#TODO settings
            Audience =
            [
                "369535811432-5n2l41tcc78ueti0tfpmppghn6jj1ucj.apps.googleusercontent.com"
            ]
        };
        GoogleJsonWebSignature.Payload? payload = await GoogleJsonWebSignature.ValidateAsync(context.Token, settings);
        AuthorizationInfo info = await _dbMongo.UpsertAuthorizationInfoAsync(
            new AuthorizationInfo(payload.Name, DateTimeOffset.Now)
            {
                AccountEnabled = true,
                AuthProvider = context.Provider,
                AuthProviderOfficial = payload.Issuer,
                CreationToken = context.Token,
                Email = payload.Email,
                ProfilePicture = payload.Picture,
                VerifiedEmail = payload.EmailVerified,
                AuthProviderSystemId = payload.Subject
            }, cancellationToken);

        ArgumentNullException.ThrowIfNull(info.ObjId);

        AuthorizationContract contract = new AuthorizationContract(info.ObjId, payload.Name, payload.Picture, context);
        return contract;
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(UserInfoContext context, CancellationToken cancellationToken)
    {
        AuthorizationInfo? info = await _dbMongo.SelectAuthorizationInfoAsync(context.Id, cancellationToken);
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(info.ObjId);
        AuthorizationContract contract =
            new AuthorizationContract(info.ObjId, info.Username, info.ProfilePicture, context);
        return contract;
    }
}