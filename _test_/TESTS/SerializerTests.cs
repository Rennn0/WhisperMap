using System.Diagnostics;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Mocking;
using Xunit.Abstractions;

namespace TESTS;

public class SerializerTests
{
    private readonly ITestOutputHelper _output;

    public SerializerTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CompressionComparison()
    {
        List<MockData> data = Data.GetMockList();
        List<CompressionResult> results = [];
        MemoryPackImplSerializer brotli = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        Stopwatch swBrotli = Stopwatch.StartNew();
        ReadOnlyMemory<byte> serialized = brotli.Serialize(data);
        swBrotli.Stop();
        ReadOnlyMemory<byte> compressed = brotli.Compress(serialized);
        results.Add(new CompressionResult("mempack", serialized.Length, compressed.Length,swBrotli.ElapsedMilliseconds));
        Stopwatch swSys = Stopwatch.StartNew();
        serialized = sys.Serialize(data);
        swSys.Stop();
        compressed = sys.Compress(serialized);
        results.Add(new CompressionResult("sys", serialized.Length, compressed.Length,swSys.ElapsedMilliseconds));
        foreach (CompressionResult r in results)
            _output.WriteLine(
                $"{r.Name,-10} | Serialized: {r.SerializedSize,6} bytes | Compressed: {r.CompressedSize,6} bytes | Ratio: {(double)r.CompressedSize / r.SerializedSize:P2} | {r.SerializationTime} ms"
            );

        Assert.All(results, r => Assert.True(r.CompressedSize > 0));
    }

    private record CompressionResult(
        string Name,
        int SerializedSize,
        int CompressedSize,
        double SerializationTime
    );
}