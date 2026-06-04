using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    /// <summary>
    ///     Georgian Lari
    /// </summary>
    [JsonStringEnumMemberName("GEL")] Gel,

    /// <summary>
    ///     United States dollar
    /// </summary>
    [JsonStringEnumMemberName("USD")] Usd,

    /// <summary>
    ///     Euro
    /// </summary>
    [JsonStringEnumMemberName("EUR")] Eur,

    /// <summary>
    ///     Armenian Dram
    /// </summary>
    [JsonStringEnumMemberName("AMD")] Amd,

    /// <summary>
    ///     Azerbaijanian Manat
    /// </summary>
    [JsonStringEnumMemberName("AZN")] Azn,

    /// <summary>
    ///     Tenge
    /// </summary>
    [JsonStringEnumMemberName("KZT")] Kzt,

    /// <summary>
    ///     Moldovian Leu
    /// </summary>
    [JsonStringEnumMemberName("MDL")] Mdl,

    /// <summary>
    ///     Uzbekistan Sum
    /// </summary>
    [JsonStringEnumMemberName("UZS")] Uzs
}