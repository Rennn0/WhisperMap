using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class UserGuard : AuthGuard
{
    private readonly ApplicationClaims[] _requiredClaims;
    private HttpContext? _context;
    private IOptionsSnapshot<IpRestrictionSettings>? _ipRestrictionSettings;

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ipRestrictionSettings"></param>
    public UserGuard(IHttpContextAccessor context, IOptionsSnapshot<IpRestrictionSettings> ipRestrictionSettings)
    {
        ArgumentNullException.ThrowIfNull(context.HttpContext);
        _context = context.HttpContext;
        _ipRestrictionSettings = ipRestrictionSettings;
        _requiredClaims = [];
    }

    /// <inheritdoc />
    public UserGuard(params ApplicationClaims[] requiredClaims)
    {
        _requiredClaims = requiredClaims;
    }

    /// <summary>
    /// </summary>
    /// <param name="userInfo"></param>
    /// <returns></returns>
    public bool TryGetUserInfo(out UserInfo? userInfo)
    {
        ArgumentNullException.ThrowIfNull(_context);
        ArgumentNullException.ThrowIfNull(_ipRestrictionSettings);
        userInfo = null;

        if (!TryGetSessionData(_context, out SessionData? sessionData)) return false;
        ArgumentNullException.ThrowIfNull(sessionData);

        userInfo = new UserInfo();

        if (_ipRestrictionSettings.Value.AllowedIpAddresses.Contains(sessionData.Ip))
        {
            userInfo.CanUpload = true;
            userInfo.CanDelete = true;
            userInfo.Claims.Add(ApplicationClaims.Upload);
            userInfo.Claims.Add(ApplicationClaims.Delete);
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
        _context = context.HttpContext;
        _ipRestrictionSettings = context.HttpContext.RequestServices
            .GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();

        if (TryGetUserInfo(out UserInfo? userInfo) &&
            userInfo is not null &&
            _requiredClaims.All(ac => userInfo.Claims.Contains(ac))) return;

        context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
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