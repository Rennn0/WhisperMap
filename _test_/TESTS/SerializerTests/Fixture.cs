using XcLib.Shared.Utils;
using XcLib.Shared.Utils.Mocking;

namespace TESTS.SerializerTests;

public class Fixture
{
    /// <summary>
    ///     1000 items
    /// </summary>
    public List<MockData> MockData { get; }

    /// <summary>
    ///     270752 byte ~ 270kb
    /// </summary>
    public byte[] MockDataBytes { get; }

    public ReadOnlyMemory<byte> MemPackBytes { get; }
    public ReadOnlyMemory<byte> SysBytes { get; }
    public ReadOnlyMemory<byte> NewonBytes { get; }

    public Fixture()
    {
        MockData = Data.GetMockList();
        MockDataBytes = Data.GetMockBytes();
        MemPackBytes = new MemoryPackImplSerializer().Serialize(MockData);
        SysBytes = new SystemJsonImplSerializer().Serialize(MockData);
        NewonBytes = new NewtonsoftJsonImplSerializer().Serialize(MockData);
    }
}