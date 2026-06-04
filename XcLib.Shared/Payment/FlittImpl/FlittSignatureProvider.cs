using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        List<string?> values = data
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => new
            {
                Name = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
                Value = GetValue(p.GetValue(data))
            })
            .Where(x =>
                !string.Equals(x.Name, "signature", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(x.Value))
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => x.Value)
            .ToList();

        string payload = string.Join("|", new[] { _configuration.PaymentKey }.Concat(values));

        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string Sign(JsonElement data)
    {
        Dictionary<string, string?> dict = new Dictionary<string, string?>();

        foreach (JsonProperty prop in data.EnumerateObject())
        {
            if (prop.NameEquals("signature"))
                continue;

            string? value = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,

                JsonValueKind.Object => prop.Value.GetRawText(),

                _ => prop.Value.GetRawText()
            };

            if (!string.IsNullOrEmpty(value))
                dict[prop.Name] = value;
        }

        List<string?> values = dict
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => x.Value)
            .ToList();

        string payload = string.Join("|", new[] { _configuration.PaymentKey }.Concat(values));

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