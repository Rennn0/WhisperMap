using MemoryPack;

namespace XcLib.Shared.Utils;

public class MemoryPackBinarySerializer : SystemJsonSerializer
{
    public override ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        byte[] bytes = MemoryPackSerializer.Serialize(value, new MemoryPackSerializerOptions
        {
            StringEncoding = StringEncoding.Utf8
        });
        return new ReadOnlyMemory<byte>(bytes);
    }


    public override T Deserialize<T>(ReadOnlyMemory<byte> bytes)
    {
        T? obj = MemoryPackSerializer.Deserialize<T>(bytes.Span, new MemoryPackSerializerOptions
        {
            StringEncoding = StringEncoding.Utf8
        });
        return obj ?? throw new InvalidOperationException();
    }
}