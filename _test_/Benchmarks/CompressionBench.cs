using System.IO.Compression;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Perfolizer.Mathematics.OutlierDetection;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Mocking;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[Outliers(OutlierMode.DontRemove)]
[RankColumn]
public class CompressionBench
{
    private readonly MemoryPackImplSerializer _mempack = new MemoryPackImplSerializer();
    private readonly byte[] _mem = Encoding.UTF8.GetBytes(Data.Json);

    [Params(
        CompressionLevel.Fastest,
        CompressionLevel.Optimal,
        CompressionLevel.SmallestSize,
        CompressionLevel.NoCompression)]
    public CompressionLevel Level { get; set; }


    [Benchmark(Baseline = true)]
    public long MempackBrotliComp()
    {
        ReadOnlyMemory<byte> compressed = _mempack.Compress(_mem);
        return compressed.Length;
    }

    [Benchmark]
    public long BrotliComp()
    {
        using MemoryStream output = new MemoryStream(_mem.Length);
        using (BrotliStream brotli = new BrotliStream(output, Level, true))
        {
            brotli.Write(_mem);
        }

        return output.Length;
    }


    [Benchmark]
    public long ZlibComp()
    {
        using MemoryStream outMs = new MemoryStream();
        using (Stream stream = new ZLibStream(outMs, Level, true))
        {
            stream.Write(_mem, 0, _mem.Length);
        }

        return outMs.Length;
    }

    [Benchmark]
    public long GzipComp()
    {
        using MemoryStream outMs = new MemoryStream();
        using (Stream stream = new GZipStream(outMs, Level, true))
        {
            stream.Write(_mem, 0, _mem.Length);
        }

        return outMs.Length;
    }
}