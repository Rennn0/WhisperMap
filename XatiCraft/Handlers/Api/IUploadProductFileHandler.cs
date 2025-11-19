using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
public interface IUploadProductFileHandler :
    IHandler<ApiContract, UploadProductFileContext>,
    IHandler<ApiContract, GetSignedUrlContext>;