namespace XcLib.Shared.Payment.Interfaces;

public interface IPaymentProvider
{
    ISignatureProvider SignatureProvider { get; }
    Task<CreatedOrder> CreateOrderAsync(CreateOrderArgs args, CancellationToken ct = default);
    Task<OrderStatus> GetOrderStatusAsync(GetOrderStatusArgs args, CancellationToken ct = default);
}