using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
public interface IAuthorizationHandler :
    IHandler<ApiContract, AuthorizationContext>,
    IHandler<ApiContract, UserInfoContext>;