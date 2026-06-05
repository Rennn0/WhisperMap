using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using XcLib.Shared.Payment.Interfaces;

namespace XcLib.Shared.Payment.FlittImpl;

public class FlittSignatureProvider : ISignatureProvider
{
    private readonly PaymentConfiguration _configuration;

    public FlittSignatureProvider(IOptions<PaymentConfiguration> options) => _configuration = options.Value;

    public string Sign(object data)
    {
        var values = data
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null)
            .Select(p => new
            {
                Name = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
                Value = GetValue(p.GetValue(data))
            })
            .Where(x =>
                !string.Equals(x.Name, "signature", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(x.Name, "response_signature_string", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(x.Value))
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToList();

        return Sign(values.Select(v => v.Value).ToList()!);
    }

    public string Sign(List<string> values)
    {
        string payload = string.Join("|", values.Prepend(_configuration.PaymentKey));
        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    private static string? GetValue(object? value)
    {
        if (value is null)
            return null;

        return value switch
        {
            Enum e => GetEnumValue(e),
            bool b => b ? "true" : "false",
            _ => value.ToString()
        };
    }

    private static string GetEnumValue(Enum value)
    {
        MemberInfo member = value.GetType().GetMember(value.ToString())[0];

        JsonStringEnumMemberNameAttribute? attr = member.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();

        return attr?.Name ?? value.ToString();
    }
}