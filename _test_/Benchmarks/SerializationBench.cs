using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Interfaces;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class SerializationBench
{
    private readonly ISerializer _mempack = new MemoryPackBinarySerializer();
    private readonly ISerializer _sysJson = new SystemJsonSerializer();
    private readonly ISerializer _newtonJson = new NewtonsoftJsonSerializer();
    private readonly List<MockData> _data = [];

    private byte[] _mempackBytes = [];
    private byte[] _sysJsonBytes = [];
    private byte[] _newtonBytes = [];


    [GlobalSetup]
    public void Setup()
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(Data.Json);
        _data.AddRange(_sysJson.Deserialize<List<MockData>>(jsonBytes));

        _mempackBytes = _mempack.Serialize(_data).ToArray();
        _sysJsonBytes = _sysJson.Serialize(_data).ToArray();
        _newtonBytes = _newtonJson.Serialize(_data).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] Mempack_Serialize()
        => _mempack.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] SystemTextJson_Serialize()
        => _sysJson.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("serialize")]
    public byte[] Newtonsoft_Serialize()
        => _newtonJson.Serialize(_data).ToArray();

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> Mempack_Deserialize()
        => _mempack.Deserialize<List<MockData>>(_mempackBytes);

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> SystemTextJson_Deserialize()
        => _sysJson.Deserialize<List<MockData>>(_sysJsonBytes);

    [Benchmark]
    [BenchmarkCategory("deserialize")]
    public List<MockData> Newtonsoft_Deserialize()
        => _newtonJson.Deserialize<List<MockData>>(_newtonBytes);
}