namespace Webhook.Objects;

[Flags]
internal enum ExecutionState
{
    None,
    Initialized,
    Started,
    Pending,
    Fail,
    Success
}