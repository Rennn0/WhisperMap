using System.Text;
using System.Text.Json;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class GetProductsGeneralHandler : IGetProductsHandler
{
    private const int MaxLenDesc = 44;
    private const int MaxLenTitle = 32;
    private readonly IProductRepo _productRepos;
    private static Security _security = null!;

    internal record Cursor : SearchCursor
    {
        public override string Encode(SearchCursor cursor) => _security.Pack(JsonSerializer.Serialize(cursor));

        public override SearchCursor? Decode(string? token) =>
            string.IsNullOrEmpty(token) ? null : JsonSerializer.Deserialize<Cursor>(_security.UnPack(token));
    }
    
    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    /// <param name="securities"></param>
    public GetProductsGeneralHandler(IEnumerable<IProductRepo> productRepos, IEnumerable<Security> securities)
    {
        _productRepos = productRepos.First(p => p is ProductRepo);
        _security = securities.First(s => s is SimpleBase64Protector);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        Cursor def = new Cursor();
        SearchCursor cursor = def.Decode(context.ContinuationToken) ?? def;
        if (context.Batch.HasValue) cursor.BatchSize = context.Batch.Value;
        
        List<Product> products =
            await _productRepos.SelectAsync(context.Ids, context.OrderBy, context.Query, cursor,
                cancellationToken);

        string? continuationToken = null;
        if (products.Count > 0)
        {
            Product last = products[^1];
            OrderBy order = context.OrderBy ?? OrderBy.NewestFirst;

            SearchCursor nextCursor = order switch
            {
                OrderBy.NewestFirst or OrderBy.OldestFirst => cursor with
                {
                    Id = last.Id, Timestamp = last.Timestamp
                },
                OrderBy.PriceDecreasing or OrderBy.PriceIncreasing => cursor with
                {
                    Id = last.Id, Price = last.Price
                },
                _ => throw new ArgumentOutOfRangeException(nameof(context), context, null)
            };

            continuationToken = def.Encode(nextCursor);
        }
        
        GetProductsContract contract = new GetProductsContract(products.Select(p =>
            new Product(EraseIfLarger(p.Title, MaxLenTitle), EraseIfLarger(p.Description, MaxLenDesc), p.Price)
            {
                Id = p.Id,
                PreviewImg = p.ProductMetadata?.Where(pm => !string.IsNullOrEmpty(pm.Location)).MinBy(pm => pm.Id)
                    ?.Location 
            }
        ), context)
        {
            ContinuationToken = continuationToken
        };
        
        return contract;
    }

    private static string EraseIfLarger(string str, int maxLen)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLen) return str;

        StringBuilder builder = new StringBuilder();
        builder.Append(str[..MaxLenDesc]);
        builder.Append("...");

        return builder.ToString();
    }
}