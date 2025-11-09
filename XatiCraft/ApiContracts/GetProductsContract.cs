using XatiCraft.Objects;

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Products"></param>
public record GetProductsContract(IEnumerable<Product> Products) : ApiContract;