using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Mocking;
using Xunit.Abstractions;

namespace TESTS.SerializerTests;

public class Main : IAsyncLifetime, IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private readonly ITestOutputHelper _output;

    public Main(ITestOutputHelper output, Fixture fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public void Serialize()
    {
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        {
            ReadOnlyMemory<byte> serialized = memPack.Serialize(_fixture.MockData);
            _output.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Serialize)} {serialized.Length}");
            Assert.True(serialized.Length > 0);
        }

        {
            ReadOnlyMemory<byte> serialized = sys.Serialize(_fixture.MockData);
            _output.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Serialize)} {serialized.Length}");
            Assert.True(serialized.Length > 0);
        }
    }

    [Fact]
    public void Deserialize()
    {
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        NewtonsoftJsonImplSerializer newton = new NewtonsoftJsonImplSerializer();
        {
            List<MockData> d = memPack.Deserialize<List<MockData>>(_fixture.MemPackBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _fixture.MockData);
        }

        {
            List<MockData> d = sys.Deserialize<List<MockData>>(_fixture.SysBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _fixture.MockData);
        }

        {
            List<MockData> d = newton.Deserialize<List<MockData>>(_fixture.NewonBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _fixture.MockData);
        }
    }

    [Fact]
    public void Compress()
    {
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        {
            ReadOnlyMemory<byte> compress = memPack.Compress(_fixture.MockDataBytes);
            _output.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _fixture.MockDataBytes.Length);
        }

        {
            ReadOnlyMemory<byte> compress = sys.Compress(_fixture.MockDataBytes);
            _output.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _fixture.MockDataBytes.Length);
        }
    }

    public Task InitializeAsync()
    {
        _output.WriteLine($"Testing {nameof(Main)}...");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _output.WriteLine($"Testing Disposed {nameof(Main)}...");
        return Task.CompletedTask;
    }
}