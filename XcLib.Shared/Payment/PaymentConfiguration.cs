namespace XcLib.Shared.Payment;

public record PaymentConfiguration
{
    public int? MerchantId { get; init; }
    public string? PaymentKey { get; init; }
    public string? CreditPrivateKey { get; init; }
    public string? ServerCallbackUrlUnformated { get; init; }
    public string? CreateOrderUrl { get; init; }
    public string? OrderStatusUrl { get; init; }
}