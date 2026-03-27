using System.Text;

namespace XatiCraft.Guards;

/// <inheritdoc />
public class SimpleBase64Protector : Security
{
    /// <inheritdoc />
    public override string Pack(string data) => Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

    /// <inheritdoc />
    public override string UnPack(string? packedData) => string.IsNullOrEmpty(packedData)
        ? string.Empty
        : Encoding.UTF8.GetString(Convert.FromBase64String(packedData));
}