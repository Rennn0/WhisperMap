using System.Reflection;
using System.Text;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public abstract class Security
{
    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract string Pack(string data);

    /// <summary>
    /// </summary>
    /// <param name="packedData"></param>
    /// <returns></returns>
    public abstract string UnPack(string? packedData);

    /// <summary>
    /// </summary>
    /// <returns></returns>
    protected static string GetProtectionPurpose()
    {
        StringBuilder builder = new StringBuilder();
        AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
        builder.Append(assemblyName.Name);
        builder.Append('_');
        builder.Append(assemblyName.Version);
        return builder.ToString();
    }
}