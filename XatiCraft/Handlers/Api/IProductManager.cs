using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

public interface IProductManager : 
    IHandler<ApiContract, CreateProductContext>,
    IHandler<ApiContract, UpdateProductContext>;