using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
public interface IProductCartHandler :
    IHandler<ApiContract, AddProductInCartContext>,
    IHandler<ApiContract, GetProductsContext>,
    IHandler<ApiContract, RemoveProductFromCartContext>;