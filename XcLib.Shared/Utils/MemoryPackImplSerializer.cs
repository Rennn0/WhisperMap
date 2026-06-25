using System.IO.Compression;
using MemoryPack;

namespace XcLib.Shared.Utils;

public class MemoryPackImplSerializer : SystemJsonImplSerializer
{
    private readonly MemoryPackSerializerOptions _options = new MemoryPackSerializerOptions
    {
        StringEncoding = StringEncoding.Utf8
    };
    public override ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        byte[] bytes = MemoryPackSerializer.Serialize(value, _options);
        return new ReadOnlyMemory<byte>(bytes);
    }


    public override T Deserialize<T>(ReadOnlyMemory<byte> bytes)
    {
        T? obj = MemoryPackSerializer.Deserialize<T>(bytes.Span, _options);
        return obj ?? throw new InvalidOperationException();
    }

    public override ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> bytes)
    {
        using MemoryStream output = new MemoryStream(bytes.Length);
        using (BrotliStream stream = new BrotliStream(output, CompressionLevel.SmallestSize, true))
        {
            stream.Write(bytes.Span);
        }

        return new ReadOnlyMemory<byte>(output.ToArray());
    }

    public override ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> bytes)
    {
        using MemoryStream msIn = new MemoryStream(bytes.ToArray());
        using MemoryStream msOut = new MemoryStream();
        using (BrotliStream stream = new BrotliStream(msIn, CompressionMode.Decompress))
        {
            stream.CopyTo(msOut);
        }

        return new ReadOnlyMemory<byte>(msOut.ToArray());
    }
}