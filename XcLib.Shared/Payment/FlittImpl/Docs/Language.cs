using System.Text.Json.Serialization;

namespace XcLib.Shared.Payment.FlittImpl.Docs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Language
{
    /// <summary>
    ///     Azerbaijani
    /// </summary>
    [JsonStringEnumMemberName("az")] Azerbaijani,

    /// <summary>
    ///     Danish
    /// </summary>
    [JsonStringEnumMemberName("da")] Danish,

    /// <summary>
    ///     Dutch
    /// </summary>
    [JsonStringEnumMemberName("nl")] Dutch,

    /// <summary>
    ///     Finnish
    /// </summary>
    [JsonStringEnumMemberName("fi")] Finnish,

    /// <summary>
    ///     Georgian
    /// </summary>
    [JsonStringEnumMemberName("ka")] Georgian,

    /// <summary>
    ///     Korean
    /// </summary>
    [JsonStringEnumMemberName("ko")] Korean,

    /// <summary>
    ///     Russian
    /// </summary>
    [JsonStringEnumMemberName("ru")] Russian,

    /// <summary>
    ///     Chinese
    /// </summary>
    [JsonStringEnumMemberName("zh")] Chinese,

    /// <summary>
    ///     Ukrainian
    /// </summary>
    [JsonStringEnumMemberName("uk")] Ukrainian,

    /// <summary>
    ///     English
    /// </summary>
    [JsonStringEnumMemberName("en")] English,

    /// <summary>
    ///     Latvian
    /// </summary>
    [JsonStringEnumMemberName("lv")] Latvian,

    /// <summary>
    ///     French
    /// </summary>
    [JsonStringEnumMemberName("fr")] French,

    /// <summary>
    ///     Czech
    /// </summary>
    [JsonStringEnumMemberName("cs")] Czech,

    /// <summary>
    ///     Romanian
    /// </summary>
    [JsonStringEnumMemberName("ro")] Romanian,

    /// <summary>
    ///     Italian
    /// </summary>
    [JsonStringEnumMemberName("it")] Italian,

    /// <summary>
    ///     Slovak
    /// </summary>
    [JsonStringEnumMemberName("sk")] Slovak,

    /// <summary>
    ///     Polish
    /// </summary>
    [JsonStringEnumMemberName("pl")] Polish,

    /// <summary>
    ///     Spanish
    /// </summary>
    [JsonStringEnumMemberName("es")] Spanish,

    /// <summary>
    ///     Hungarian
    /// </summary>
    [JsonStringEnumMemberName("hu")] Hungarian,

    /// <summary>
    ///     German
    /// </summary>
    [JsonStringEnumMemberName("de")] German
}