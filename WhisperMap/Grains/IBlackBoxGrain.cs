using Orleans.Concurrency;
using WhisperMap.Models;

namespace WhisperMap.Grains;

[Alias("IBlackBoxGrain")]
public interface IBlackBoxGrain : IGrainWithStringKey
{
    [Alias("GetStatisticsAsync")]
    ValueTask<CommonStatistics> GetStatisticsAsync();
}

[Alias("ITextBoxGrain")]
public interface ITextBoxGrain : IBlackBoxGrain
{
    [OneWay]
    [Alias("PackAsync")]
    ValueTask PackAsync(int pedestrian, int pin, string text);

    [Alias("UnPackAsync")]
    ValueTask<string> UnPackAsync(int pedestrian);
}

[Alias("IAudioBoxGrain")]
public interface IAudioBoxGrain : IBlackBoxGrain
{
    [OneWay]
    [Alias("PackAsync")]
    ValueTask PackAsync(int pedestrian, int pin, byte[] audio);

    [Alias("UnPackAsync")]
    ValueTask<byte[]> UnPackAsync(int pedestrian);
}