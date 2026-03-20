using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

internal class ProductCartHandlerMongo :
    IHandler<ApiContract, AddProductInCartContext>,
    IHandler<ApiContract, RemoveProductFromCartContext>
{
    private readonly IProductCartRepo _productCartRepo;

    public ProductCartHandlerMongo(IProductCartRepo productCartRepo)
    {
        _productCartRepo = productCartRepo;
    }

    public async ValueTask<ApiContract> HandleAsync(AddProductInCartContext context,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(context.UserId))
            await _productCartRepo.UpsertAsync(
                new ProductCart(context.UserId) { ProductIds = [context.ProductId.ToString()] },
                cancellationToken);

        return new ApiContract(context);
    }

    public async ValueTask<ApiContract>
        HandleAsync(RemoveProductFromCartContext context, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(context.UserId))
            await _productCartRepo.RemoveAsync(context.UserId, context.ProductId.ToString(), cancellationToken);
        ApiContract contract = new ApiContract(context);
        return contract;
    }
}