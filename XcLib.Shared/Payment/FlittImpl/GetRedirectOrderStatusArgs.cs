namespace XcLib.Shared.Payment.FlittImpl;

public record GetRedirectOrderStatusArgs(string InternalOrderId) : GetOrderStatusArgs;