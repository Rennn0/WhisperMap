using Moq;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Interfaces;
using XcLib.Shared.Utils.Mocking;

namespace TESTS.SerializerTests;

public class Main : IAsyncLifetime, IClassFixture<SerializerFixture>
{
    private readonly SerializerFixture _serializerFixture;
    private readonly ITestOutputHelper _output;
    private readonly Mock<ISerializer> _serializerMock;

    public Main(ITestOutputHelper output, SerializerFixture serializerFixture)
    {
        _output = output;
        _serializerFixture = serializerFixture;
        _serializerMock = new Mock<ISerializer>(MockBehavior.Default);
    }

    public static TheoryData<int> Data() => new TheoryData<int>(1, 2, 3);

    public static IEnumerable<TheoryDataRow<List<int>>> DataList()
    {
        yield return new TheoryDataRow<List<int>>([1, 2, 3]);
        yield return new TheoryDataRow<List<int>>([5, 6, 7]);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void Serialize(int data)
    {
        Console.WriteLine(data);
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        {
            ReadOnlyMemory<byte> serialized = memPack.Serialize(_serializerFixture.MockData);
            _output.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Serialize)} {serialized.Length}");
            Assert.True(serialized.Length > 0);
        }

        {
            ReadOnlyMemory<byte> serialized = sys.Serialize(_serializerFixture.MockData);
            _output.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Serialize)} {serialized.Length}");
            Assert.True(serialized.Length > 0);
        }
    }

    [Theory]
    [MemberData(nameof(DataList))]
    public void Deserialize(List<int> data)
    {
        Console.WriteLine(string.Join('_', data));
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        NewtonsoftJsonImplSerializer newton = new NewtonsoftJsonImplSerializer();
        {
            List<MockData> d = memPack.Deserialize<List<MockData>>(_serializerFixture.MemPackBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _serializerFixture.MockData);
        }

        {
            List<MockData> d = sys.Deserialize<List<MockData>>(_serializerFixture.SysBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _serializerFixture.MockData);
        }

        {
            List<MockData> d = newton.Deserialize<List<MockData>>(_serializerFixture.NewonBytes);
            Assert.True(d.Count > 0);
            Assert.Equal(d, _serializerFixture.MockData);
        }
    }

    [Fact]
    public void Compress()
    {
        // _serializerMock.Setup(x => x.Compress(new ReadOnlyMemory<byte>())).Returns(new ReadOnlyMemory<byte>([]));
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        {
            ReadOnlyMemory<byte> compress = memPack.Compress(_serializerFixture.MockDataBytes);
            _output.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _serializerFixture.MockDataBytes.Length);
        }

        {
            ReadOnlyMemory<byte> compress = sys.Compress(_serializerFixture.MockDataBytes);
            _output.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _serializerFixture.MockDataBytes.Length);
        }
    }

    public ValueTask InitializeAsync()
    {
        _output.WriteLine($"Testing {nameof(Main)}...");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _output.WriteLine($"Testing Disposed {nameof(Main)}...");
        return ValueTask.CompletedTask;
    }
}