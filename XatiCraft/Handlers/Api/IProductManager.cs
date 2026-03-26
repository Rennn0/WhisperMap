using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <inheritdoc />
public interface IProductManager : 
    IHandler<ApiContract, CreateProductContext>,
    IHandler<ApiContract, UpdateProductContext>;