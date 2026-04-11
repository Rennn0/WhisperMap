namespace XcLib.Data.Abstractions;

/// <summary>
/// </summary>
[Flags]
public enum OrderBy
{
    /// <summary>
    /// </summary>
    NewestFirst,

    /// <summary>
    /// </summary>
    OldestFirst,

    /// <summary>
    /// </summary>
    PriceIncreasing,

    /// <summary>
    /// </summary>
    PriceDecreasing
}