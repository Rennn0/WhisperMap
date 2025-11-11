using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class UserManager : AuthGuard
{
    private readonly HttpContext _context;
    private readonly IOptionsSnapshot<IpRestrictionSettings> _ipRestrictionSettings;

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ipRestrictionSettings"></param>
    public UserManager(IHttpContextAccessor context, IOptionsSnapshot<IpRestrictionSettings> ipRestrictionSettings)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);
        _context = context.HttpContext;
        _ipRestrictionSettings = ipRestrictionSettings;
    }

    /// <summary>
    /// </summary>
    /// <param name="userInfo"></param>
    /// <returns></returns>
    public bool TryGetUserInfo(out UserInfo? userInfo)
    {
        userInfo = null;
        if (!TryGetSessionData(_context, out SessionData? sessionData)) return false;
        ArgumentNullException.ThrowIfNull(sessionData);

        userInfo = new UserInfo();

        if (_ipRestrictionSettings.Value.AllowedIpAddresses.Contains(sessionData.Ip))
        {
            userInfo.CanUpload = true;
            userInfo.Claims.Add(ApplicationClaims.Upload);
        }

        userInfo.Claims.Add(ApplicationClaims.Read);
        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        throw new NotImplementedException();
    }
}