using System.ComponentModel.DataAnnotations;

namespace XcLib.Sse.Options;

public class SseOptions
{
    [Range(1, uint.MaxValue)] public uint PingInterval { get; init; }
}