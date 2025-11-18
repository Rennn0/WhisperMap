using Microsoft.AspNetCore.DataProtection;

namespace XatiCraft.Guards;

/// <inheritdoc />
public class AspDataProtector : Security
{
    private readonly IDataProtector _protector;

    /// <inheritdoc />
    public AspDataProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(GetProtectionPurpose());
    }

    /// <inheritdoc />
    public override string Pack(string data)
    {
        return _protector.Protect(data);
    }

    /// <inheritdoc />
    public override string UnPack(string? packedData)
    {
        return string.IsNullOrEmpty(packedData) ? string.Empty : _protector.Unprotect(packedData);
    }
}