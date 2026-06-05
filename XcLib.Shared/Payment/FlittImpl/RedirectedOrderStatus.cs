using XcLib.Shared.Payment.FlittImpl.Docs;

namespace XcLib.Shared.Payment.FlittImpl;

public record RedirectedOrderStatus(
    string InternalOrderId,
    int Amount,
    Currency? Currency,
    string? Card,
    string? Status,
    DateTime? Time)
    : OrderStatus;