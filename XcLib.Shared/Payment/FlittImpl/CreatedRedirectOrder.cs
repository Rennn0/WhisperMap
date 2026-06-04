namespace XcLib.Shared.Payment.FlittImpl;

public record CreatedRedirectOrder(string CheckoutUrl, string InternalOrderId, string? Error = null) : CreatedOrder;