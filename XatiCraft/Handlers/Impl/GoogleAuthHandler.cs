using System.Net.Http.Headers;
using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.MongoImpl;
using XatiCraft.Handlers.Api;
using XatiCraft.Settings;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
public class GoogleAuthHandler : IAuthorizationHandler
{
    private readonly GoogleAuthSettings _googleAuthSettings;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// </summary>
    protected readonly IAuthorizationRepo AuthorizationRepo;

    /// <summary>
    /// </summary>
    /// <param name="repos"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="googleAuthSettings"></param>
    // ReSharper disable once MemberCanBeProtected.Global
    public GoogleAuthHandler(IEnumerable<IAuthorizationRepo> repos, IHttpClientFactory httpClientFactory,
        IOptionsMonitor<GoogleAuthSettings> googleAuthSettings)
    {
        _googleAuthSettings = googleAuthSettings.CurrentValue;
        AuthorizationRepo = repos.First(r => r is AuthorizationRepo);
        _httpClient = httpClientFactory.CreateClient(nameof(GoogleAuthHandler));
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public virtual async ValueTask<ApiContract> HandleAsync(AuthorizationContext context,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage tokenResponse = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _googleAuthSettings.ClientId,
                ["client_secret"] = _googleAuthSettings.ClientSecret,
                ["code"] = context.Token,
                ["redirect_uri"] = _googleAuthSettings.Redirect,
                ["grant_type"] = "authorization_code"
            }),
            cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        JsonDocument json = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
        string token = json.RootElement.GetProperty("id_token").GetString() ?? throw new ArgumentNullException();
        GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience =
            [
                _googleAuthSettings.ClientId
            ]
        };
        GoogleJsonWebSignature.Payload? payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
        AuthorizationInfo info = await AuthorizationRepo.UpsertAuthorizationInfoAsync(
            new AuthorizationInfo(payload.Name, DateTimeOffset.Now)
            {
                AccountEnabled = true,
                AuthProvider = context.Provider.ToString(),
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
    public virtual async ValueTask<ApiContract> HandleAsync(UserInfoContext context,
        CancellationToken cancellationToken)
    {
        AuthorizationInfo? info = await AuthorizationRepo.SelectAuthorizationInfoAsync(context.Id, cancellationToken);
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(info.ObjId);
        AuthorizationContract contract =
            new AuthorizationContract(info.ObjId, info.Username, info.ProfilePicture, context);
        return contract;
    }
}