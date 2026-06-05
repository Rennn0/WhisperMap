using XcLib.Shared.Payment.FlittImpl.Docs;

namespace XcLib.Shared.Payment.FlittImpl;

public record RedirectedOrderStatus(
    string InternalOrderId,
    int Amount,
    Currency? Currency,
    string? Card,
    DateTime? Time)
    : OrderStatus;