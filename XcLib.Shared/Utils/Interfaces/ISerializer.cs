namespace XcLib.Shared.Utils.Interfaces;

public interface ISerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T value);
    T Deserialize<T>(ReadOnlyMemory<byte> bytes);
    ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> bytes);
    ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> bytes);
}