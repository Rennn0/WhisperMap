using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Mathematics.OutlierDetection;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Mocking;

namespace Benchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[Outliers(OutlierMode.DontRemove)]
public class SerializationBench
{
    private readonly MemoryPackImplSerializer _mempack = new MemoryPackImplSerializer();
    private readonly SystemJsonImplSerializer _sysJsonImpl = new SystemJsonImplSerializer();
    private readonly NewtonsoftJsonImplSerializer _newtonJsonImpl = new NewtonsoftJsonImplSerializer();
    private readonly List<MockData> _data = [];

    private byte[] _mempackBytes = [];
    private byte[] _sysJsonBytes = [];
    private byte[] _newtonBytes = [];


    [GlobalSetup]
    public void Setup()
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(Data.Json);
        _data.AddRange(_sysJsonImpl.Deserialize<List<MockData>>(jsonBytes));

        _mempackBytes = _mempack.Serialize(_data).ToArray();
        _sysJsonBytes = _sysJsonImpl.Serialize(_data).ToArray();
        _newtonBytes = _newtonJsonImpl.Serialize(_data).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] Mempack_Serialize()
        => _mempack.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] SystemTextJson_Serialize()
        => _sysJsonImpl.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] Newtonsoft_Serialize()
        => _newtonJsonImpl.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> Mempack_Deserialize()
        => _mempack.Deserialize<List<MockData>>(_mempackBytes);

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> SystemTextJson_Deserialize()
        => _sysJsonImpl.Deserialize<List<MockData>>(_sysJsonBytes);

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> Newtonsoft_Deserialize()
        => _newtonJsonImpl.Deserialize<List<MockData>>(_newtonBytes);
}