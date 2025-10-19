using Orleans.Runtime;
using WhisperMap.Models;

namespace WhisperMap.Grains;

public class AudioBoxGrain : Grain, IAudioBoxGrain
{
    private readonly IPersistentState<AudioBoxState> _blackBoxState;

    public AudioBoxGrain([PersistentState(nameof(AudioBoxState))] IPersistentState<AudioBoxState> blackBoxState)
    {
        _blackBoxState = blackBoxState;
    }

    public ValueTask PackAsync(int pedestrian, int pin, byte[] audio)
    {
        _blackBoxState.State = new AudioBoxState
        {
            BoxOwner = pedestrian,
            Audio = audio,
            PinId = pin
        };

        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[]> UnPackAsync(int pedestrian)
    {
        _blackBoxState.State.UnboxedBy.Add(pedestrian);
        return ValueTask.FromResult(_blackBoxState.State.Audio);
    }

    public ValueTask<CommonStatistics> GetStatisticsAsync()
    {
        return ValueTask.FromResult(new CommonStatistics(_blackBoxState.State.UnboxedBy.Count));
    }
}