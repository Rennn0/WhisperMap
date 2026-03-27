using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class GetProductsGeneralHandler : IGetProductsHandler
{
    private const int MaxLenDesc = 44;
    private const int MaxLenTitle = 32;
    private const uint MaxBatchSize = 50;
    private const uint DefaultBatchSize = 5;
    private readonly IProductRepo _productRepos;

    internal sealed record SearchCursor
    {
        private uint? _batchSize;
        [JsonPropertyName("0")] public long? Id { get; init; }

        [JsonPropertyName("1")]
        public uint BatchSize
        {
            get => _batchSize ?? DefaultBatchSize;
            set => _batchSize = Math.Min(value, MaxBatchSize);
        }

        [JsonPropertyName("2")] public DateTime? Timestamp { get; init; }
        [JsonPropertyName("3")] public decimal? Price { get; init; }

        internal static string Encode(SearchCursor cursor)
        {
            string json = JsonSerializer.Serialize(cursor);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        internal static SearchCursor? Decode(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(token);
                string json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<SearchCursor>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    public GetProductsGeneralHandler(IEnumerable<IProductRepo> productRepos)
    {
        _productRepos = productRepos.First(p => p is ProductRepo);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        SearchCursor cursor = SearchCursor.Decode(context.ContinuationToken) ??
                               new SearchCursor();
        cursor.BatchSize = context.Batch ?? DefaultBatchSize;
        
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

            continuationToken = SearchCursor.Encode(nextCursor);
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