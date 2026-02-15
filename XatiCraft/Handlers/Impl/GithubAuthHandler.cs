using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Settings;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class GithubAuthHandler : GoogleAuthHandler
{
    private readonly GithubAuthSettings _githubSettings;
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public GithubAuthHandler(IEnumerable<IAuthorizationRepo> repos, IHttpClientFactory httpClientFactory,
        IOptionsMonitor<GithubAuthSettings> githubSettings,
        IOptionsMonitor<GoogleAuthSettings> googleAuthSettings) : base(repos, httpClientFactory, googleAuthSettings)
    {
        _githubSettings = githubSettings.CurrentValue;
        _httpClient = httpClientFactory.CreateClient(nameof(GithubAuthHandler));
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public override async ValueTask<ApiContract> HandleAsync(AuthorizationContext context,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage tokenResponse = await _httpClient.PostAsJsonAsync(
            new Uri("https://github.com/login/oauth/access_token"),
            new
            {
                client_id = _githubSettings.ClientId,
                client_secret = _githubSettings.ClientSecret,
                code = context.Token
            }, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        JsonDocument tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
        string accessToken = tokenJson.RootElement.GetProperty("access_token").GetString() ??
                             throw new ArgumentNullException();

        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.github.com/user"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd(_githubSettings.UserAgent);

        using HttpResponseMessage userResponse = await _httpClient.SendAsync(request, cancellationToken);
        userResponse.EnsureSuccessStatusCode();
        JsonDocument userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync(cancellationToken));
        long id = userJson.RootElement.GetProperty("id").GetInt64();
        string name = userJson.RootElement.GetProperty("name").GetString() ?? throw new Exception();
        string? email = userJson.RootElement.TryGetProperty("email", out JsonElement e) ? e.GetString() : null;
        string? profilePic = userJson.RootElement.TryGetProperty("avatar_url", out JsonElement e2)
            ? e2.GetString()
            : null;

        AuthorizationInfo info = await AuthorizationRepo.UpsertAuthorizationInfoAsync(
            new AuthorizationInfo(name, DateTimeOffset.Now)
            {
                AccountEnabled = true,
                AuthProvider = context.Provider.ToString(),
                Email = email,
                AuthProviderSystemId = id.ToString(),
                ProfilePicture = profilePic,
                VerifiedEmail = profilePic is not null
            }, cancellationToken);

        ArgumentNullException.ThrowIfNull(info.ObjId);
        AuthorizationContract contract =
            new AuthorizationContract(info.ObjId, info.Username, info.ProfilePicture, context);
        return contract;
    }
}