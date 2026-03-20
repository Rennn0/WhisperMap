using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

internal class ProductCartHandlerMongo : IHandler<ApiContract, AddProductInCartContext>
{
    private readonly IProductCartRepo _productCartRepo;
    private readonly HttpContext _httpContext;

    public ProductCartHandlerMongo(IProductCartRepo productCartRepo, IHttpContextAccessor httpContextAccessor)
    {
        _productCartRepo = productCartRepo;
        _httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async ValueTask<ApiContract> HandleAsync(AddProductInCartContext context,
        CancellationToken cancellationToken)
    {
        if (_httpContext.Request.Cookies.TryGetValue(AuthGuard.UserIdCookie, out string? uid))
            await _productCartRepo.UpsertAsync(new ProductCart(uid) { ProductIds = [context.ProductId.ToString()] },
                cancellationToken);

        return new ApiContract(context);
    }
}