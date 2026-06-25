using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace XcLib.Shared.Utils;

public class TokenProvider
{
    private readonly TokenOptions _options;
    private readonly TimeSpan _expire;
    private const int DefaultExpireSec = 3600;
    private const string DefaultIssuer = "01KTK4XJRCNM8Q8088K8WMYEC2";
    private const string DefaultAudience = "01KTK527HFTAG02NNXW45T7ZV7";
    private const string DefaultKey = "5JuZJRvrML39oyb6F8gpPreYVrUqXZaFGYbUfcvFzBhuTUd8cK41a37AujqGe94k";
    public const string ClaimTypePermission = "permission";
    public const string ClaimTypeRole = "role";
    public const string ClaimTypeEmail = JwtRegisteredClaimNames.Email;
    public const string ClaimTypeName = JwtRegisteredClaimNames.Name;
    public const string ClaimTypeSub = JwtRegisteredClaimNames.Sub;

    public TokenProvider(IOptions<TokenOptions> options)
    {
        _options = options.Value;
        _expire = TimeSpan.FromSeconds(_options.ExpireSec ?? DefaultExpireSec);
    }

    public TokenProvider() : this(Options.Create(new TokenOptions
    {
        ExpireSec = 1,
        Key = DefaultKey,
        Issuer = DefaultIssuer,
        Audience = DefaultAudience
    }))
    {
    }

    public string Issuer => _options.Issuer ?? throw new InvalidOperationException();
    public string Audience => _options.Audience ?? throw new InvalidOperationException();
    public string Key => _options.Key ?? throw new InvalidOperationException();

    public string Create(string userId, string username, string email,
        HashSet<string>? roles = null,
        HashSet<string>? permissions = null,
        Dictionary<string, string>? additional = null,
        TimeSpan? expire = null)
    {
        ArgumentNullException.ThrowIfNull(_options.Key);

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        List<Claim> claims =
        [
            new Claim(ClaimTypeSub, userId),
            new Claim(ClaimTypeName, username),
            new Claim(ClaimTypeEmail, email)
        ];

        if (roles is { Count: > 0 }) claims.AddRange(roles.Select(role => new Claim(ClaimTypeRole, role)));
        if (permissions is { Count: > 0 })
            claims.AddRange(permissions.Select(permission => new Claim(ClaimTypePermission, permission)));
        if (additional is { Count: > 0 })
            claims.AddRange(additional.Select(a => new Claim(a.Key, a.Value)));

        JwtSecurityToken token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.Add(expire ?? _expire),
            creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}