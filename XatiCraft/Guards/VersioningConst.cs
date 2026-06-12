namespace XatiCraft.Guards;

internal struct VersioningConst
{
    //#WARN mustbe number, ascending order
    private const string V = "1";

    /// <summary>
    ///     client must have cookie
    /// </summary>
    public const string SessionCookie = $"__xc_se_{V}";

    /// <summary>
    /// </summary>
    public const string UserIdCookie = $"__xc_uid_{V}";

    /// <summary>
    /// </summary>
    public const string CartItemsCookie = $"__xc_pcc_{V}";

    /// <summary>
    ///     policy for rate limiting by session
    /// </summary>
    public const string SessionPolicy = "__xc_se_policy";

    /// <summary>
    ///     must header for initializing session cookie
    /// </summary>
    public const string IpHeader = "x-public-ip";
}