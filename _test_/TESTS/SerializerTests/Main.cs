using Moq;
using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Interfaces;
using XcLib.Shared.Utils.Mocking;

namespace TESTS.SerializerTests;

public class Main : IClassFixture<SerializerFixture>
{
    private readonly SerializerFixture _serializerFixture;
    private readonly ITestOutputHelper _outputHelper;
    private readonly Mock<ISerializer> _serializerMock;

    public Main(ITestOutputHelper outputHelper, SerializerFixture serializerFixture)
    {
        _outputHelper = outputHelper;
        _serializerFixture = serializerFixture;
        _serializerMock = new Mock<ISerializer>(MockBehavior.Default);
    }

    [Fact]
    public void Serialize()
    {
        MemoryPackImplSerializer memPack = new MemoryPackImplSerializer();
        SystemJsonImplSerializer sys = new SystemJsonImplSerializer();
        {
            ReadOnlyMemory<byte> serialized = memPack.Serialize(_serializerFixture.MockData);
            _outputHelper.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Serialize)} {serialized.Length}");
            Assert.True(serialized.Length > 0);
        }

        {
            ReadOnlyMemory<byte> serialized = sys.Serialize(_serializerFixture.MockData);
            _outputHelper.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Serialize)} {serialized.Length}");
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
            _outputHelper.WriteLine($"{nameof(MemoryPackImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _serializerFixture.MockDataBytes.Length);
        }

        {
            ReadOnlyMemory<byte> compress = sys.Compress(_serializerFixture.MockDataBytes);
            _outputHelper.WriteLine($"{nameof(SystemJsonImplSerializer)} {nameof(Compress)} {compress.Length}");
            Assert.True(compress.Length > 0);
            Assert.True(compress.Length < _serializerFixture.MockDataBytes.Length);
        }
    }
}