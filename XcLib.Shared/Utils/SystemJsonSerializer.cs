using System.IO.Compression;
using System.Text.Json;
using XcLib.Shared.Utils.Interfaces;

namespace XcLib.Shared.Utils;

public class SystemJsonSerializer : ISerializer
{
    public virtual ReadOnlyMemory<byte> Serialize<T>(T value) =>
        new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(value));

    public virtual T Deserialize<T>(ReadOnlyMemory<byte> bytes) => JsonSerializer.Deserialize<T>(bytes.Span) ?? throw new InvalidOperationException();

    public virtual ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> bytes)
    {
        using MemoryStream output = new MemoryStream(bytes.Length);
        using (BrotliStream brotli = new BrotliStream(output, CompressionLevel.SmallestSize, true))
        {
            brotli.Write(bytes.Span);
        }

        return new ReadOnlyMemory<byte>(output.ToArray());
    }

    public virtual ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> bytes)
    {
        using MemoryStream msIn = new MemoryStream(bytes.ToArray());
        using MemoryStream msOut = new MemoryStream();
        using (BrotliStream brotli = new BrotliStream(msIn, CompressionMode.Decompress))
        {
            brotli.CopyTo(msOut);
        }

        return new ReadOnlyMemory<byte>(msOut.ToArray());
    }
}