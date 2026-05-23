using System.Threading.Tasks.Dataflow;

namespace XcLib.Shared.Dataflow;

public class MeshOptions
{
    public readonly DataflowLinkOptions DataflowLinkOptions = new DataflowLinkOptions
    {
        PropagateCompletion = true,
        Append = true
    };

    public readonly DataflowBlockOptions DataflowBlockOptions = new DataflowBlockOptions
    {
        EnsureOrdered = true,
        BoundedCapacity = 100,
        MaxMessagesPerTask = 10
    };

    public readonly ExecutionDataflowBlockOptions ExecutionDataflowBlockOptions = new ExecutionDataflowBlockOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        EnsureOrdered = true,
        BoundedCapacity = 100
    };
}