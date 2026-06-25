using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using XcLib.Shared.Utils;

namespace TESTS.TokenProviderTests;

public class Main
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TokenProvider _tokenProvider;

    public Main(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        string key = Guid.NewGuid().ToString("N");
        _tokenProvider = new TokenProvider(Options.Create(new TokenOptions
        {
            Issuer = "main_test_tp_issuer",
            Audience = "main_test_tp_audience",
            ExpireSec = 60,
            Key = key
        }));
    }

    [Fact]
    public void TokenProvider_Create_Minimal_Ok()
    {
        string token = _tokenProvider.Create("abc", "def", "ghi");
        JwtSecurityToken? tokenHandler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        _outputHelper.WriteLine(tokenHandler.ToString());
        Assert.NotNull(tokenHandler);
        Assert.True(token.Length > 0);
        Assert.Equal("abc", tokenHandler.GetClaim(TokenProvider.ClaimTypeSub));
        Assert.Equal("def", tokenHandler.GetClaim(TokenProvider.ClaimTypeName));
        Assert.Equal("ghi", tokenHandler.GetClaim(TokenProvider.ClaimTypeEmail));
        Assert.Equal("main_test_tp_issuer", _tokenProvider.Issuer);
        Assert.Equal("main_test_tp_audience", _tokenProvider.Audience);
        Assert.Equal(tokenHandler.ValidTo - tokenHandler.ValidFrom, TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void TokenProvider_Create_WithClaimsAndRole_Ok()
    {
        HashSet<string> roles = ["r1", "r2"];
        HashSet<string> permissions = ["x:create", "x:update"];
        Dictionary<string, string> additional = new Dictionary<string, string>
        {
            ["foo"] = "bar"
        };
        string token = _tokenProvider.Create("abc", "def", "ghi", roles, permissions, additional);
        JwtSecurityToken? tokenHandler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        _outputHelper.WriteLine(tokenHandler.ToString());
        Assert.NotNull(tokenHandler);
        Assert.True(token.Length > 0);
        Assert.Equal(2, tokenHandler.GetClaims(TokenProvider.ClaimTypeRole).Count);
        Assert.Equal(2, tokenHandler.GetClaims(TokenProvider.ClaimTypePermission).Count);
        Assert.Equal("bar", tokenHandler.GetClaim("foo"));
    }

    [Fact]
    public void TokenProvider_Create_WithoutKey_Throws()
    {
        TokenProvider provider = new TokenProvider(Options.Create(new TokenOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            Key = null
        }));

        Assert.Throws<ArgumentNullException>(() =>
            provider.Create("abc", "def", "ghi"));
    }

    [Fact]
    public void TokenProvider_Token_WithWrongKey_Throws()
    {
        string token = _tokenProvider.Create("abc", "def", "ghi");

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
        {
            handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        "5JuZJRvrML39oyb6F8gpPreYVrUqXZaFGYbUfcvFzBhuTUd8cK41a37AujqGe94k"u8.ToArray())
                },
                out _);
        });
    }
}

file static class Helper
{
    public static string GetClaim(this JwtSecurityToken jwt, string claimType)
        => jwt.Claims.First(x => x.Type == claimType).Value;

    public static List<string> GetClaims(
        this JwtSecurityToken jwt,
        string claimType) =>
        jwt.Claims
            .Where(x => x.Type == claimType)
            .Select(x => x.Value).ToList();
}