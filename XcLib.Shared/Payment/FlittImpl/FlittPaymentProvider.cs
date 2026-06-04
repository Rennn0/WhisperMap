using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using XcLib.Shared.Payment.FlittImpl.Docs;
using XcLib.Shared.Payment.Interfaces;

namespace XcLib.Shared.Payment.FlittImpl;

public class FlittPaymentProvider : IPaymentProvider
{
    private readonly PaymentConfiguration _configuration;
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FlittPaymentProvider(IEnumerable<ISignatureProvider> signatureProviders,
        IOptions<PaymentConfiguration> options, IHttpClientFactory httpClientFactory)
    {
        SignatureProvider = signatureProviders.First(sp => sp is FlittSignatureProvider);
        _configuration = options.Value;
        _http = httpClientFactory.CreateClient(nameof(FlittPaymentProvider));
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public ISignatureProvider SignatureProvider { get; }

    public async Task<CreatedOrder> CreateOrderAsync(CreateOrderArgs args, CancellationToken ct = default)
    {
        if (args is not CreateRedirectOrderArgs createArgs) throw new InvalidOperationException();
        ArgumentException.ThrowIfNullOrEmpty(_configuration.CreateOrderUrl);

        CreateOrderRequest request = GetRequest(createArgs);
        (CreateOrderResponse? model, string json) =
            await GetHttpResponse<CreateOrderResponse>(request, _configuration.CreateOrderUrl, ct);

        CreateOrderResponseData response = model?.Response ?? throw new InvalidOperationException();

        return new CreatedRedirectOrder(response.CheckoutUrl ?? "", request.Request.OrderId,
            response.ErrorMessage);
    }

    public async Task<OrderStatus> GetOrderStatusAsync(GetOrderStatusArgs args, CancellationToken ct = default)
    {
        if (args is not GetRedirectOrderStatusArgs getArgs) throw new InvalidOperationException();
        ArgumentException.ThrowIfNullOrEmpty(_configuration.OrderStatusUrl);

        GetOrderStatusRequest request = GetRequest(getArgs);
        (GetOrderStatusResponse? model, string json) =
            await GetHttpResponse<GetOrderStatusResponse>(request, _configuration.OrderStatusUrl, ct);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement responseElement = doc.RootElement.GetProperty("response");
        string sig = SignatureProvider.Sign(responseElement);

        GetOrderStatusResponseData data = model?.Response ?? throw new InvalidOperationException();

        return new RedirectedOrderStatus(
            data.OrderId,
            int.TryParse(data.Amount, out int a) ? a : -1,
            data.Currency,
            data.MaskedCard,
            DateTime.TryParse(data.OrderTime, out DateTime t) ? t : null);
    }

    private CreateOrderRequest GetRequest(CreateRedirectOrderArgs args)
    {
        ArgumentNullException.ThrowIfNull(_configuration.MerchantId);
        ArgumentException.ThrowIfNullOrEmpty(_configuration.ServerCallbackUrlUnformated);

        string internalOrderid = Guid.NewGuid().ToString("N");
        CreateOrderRequestData requestData = new CreateOrderRequestData
        {
            MerchantId = _configuration.MerchantId.Value,
            ServerCallbackUrl = string.Format(_configuration.ServerCallbackUrlUnformated, internalOrderid),
            OrderId = internalOrderid,
            Amount = args.Amount,
            Currency = args.Currency,
            OrderDescription = args.OrderDescription
        };
        requestData.Signature = SignatureProvider.Sign(requestData);
        return new CreateOrderRequest
        {
            Request = requestData
        };
    }

    private GetOrderStatusRequest GetRequest(GetRedirectOrderStatusArgs args)
    {
        ArgumentNullException.ThrowIfNull(_configuration.MerchantId);

        GetOrderStatusRequestData requestData = new GetOrderStatusRequestData
        {
            MerchantId = _configuration.MerchantId.Value,
            OrderId = args.InternalOrderId
        };
        requestData.Signature = SignatureProvider.Sign(requestData);
        return new GetOrderStatusRequest
        {
            Request = requestData
        };
    }

    private async Task<(T? model, string json)> GetHttpResponse<T>(object request, string url, CancellationToken ct)
    {
        HttpResponseMessage res =
            await _http.PostAsJsonAsync(url, request, _jsonSerializerOptions, ct);
        res.EnsureSuccessStatusCode();
        string content = await res.Content.ReadAsStringAsync(ct);
        T? model = JsonSerializer.Deserialize<T>(content);
        return (model, content);
    }
}