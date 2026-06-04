using XcLib.Shared.Payment.FlittImpl.Docs;

namespace XcLib.Shared.Payment.FlittImpl;

public record CreateRedirectOrderArgs(int Amount, Currency Currency, string OrderDescription) : CreateOrderArgs;