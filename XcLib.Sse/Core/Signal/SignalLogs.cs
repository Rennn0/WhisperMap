using Microsoft.Extensions.Logging;

namespace XcLib.Sse.Core.Signal;

internal static partial class SignalLogs
{
    private enum SignalLogId
    {
        Wait = 200,
        CancelledWaiter,
        CompletedWaiters,
        ExcCannotAdd,
        Dispose
    }

    [LoggerMessage(LogLevel.Error, "cant add waiter {Guid}", EventId = (int)SignalLogId.ExcCannotAdd)]
    internal static partial void LogCantAddWaiterGuid(this ILogger logger, Guid guid);

    [LoggerMessage(LogLevel.Debug, "add waiter {Guid}, total waiters {Total}", EventId = (int)SignalLogId.Wait)]
    internal static partial void LogAddWaiterGuidTotalWaitersTotal(this ILogger logger, Guid guid, int total);

    [LoggerMessage(LogLevel.Debug, "cancelled waiter {Guid}, remaining waiters {Remaining}",
        EventId = (int)SignalLogId.CancelledWaiter)]
    internal static partial void LogCancelledWaiterGuidRemainingWaitersRemaining(this ILogger logger, Guid guid,
        int remaining);

    [LoggerMessage(LogLevel.Debug, "delivered to {Delivered} waiter, remaining {Remaining}",
        EventId = (int)SignalLogId.CompletedWaiters)]
    internal static partial void LogDeliveredToDeliveredWaiterRemainingRemaining(this ILogger logger, int delivered,
        int remaining);

    [LoggerMessage(LogLevel.Debug, "disposed signal, waiters {Waiters}", EventId = (int)SignalLogId.Dispose)]
    internal static partial void LogDisposedSignalWaitersWaiters(this ILogger logger, int waiters);
}