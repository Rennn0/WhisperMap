using Orleans.Runtime;
using WhisperMap.Models;

namespace WhisperMap.Grains;

public class TextBoxGrain : Grain, ITextBoxGrain
{
    private readonly IPersistentState<BlackBoxState> _blackBoxState;

    public TextBoxGrain([PersistentState(nameof(BlackBoxState))] IPersistentState<BlackBoxState> blackBoxState)
    {
        _blackBoxState = blackBoxState;
    }

    public ValueTask PackAsync(int pedestrian, int pin, string text)
    {
        _blackBoxState.State = new BlackBoxState
        {
            BoxOwner = pedestrian,
            PinId = pin,
            Description = text
        };

        return ValueTask.CompletedTask;
    }

    public ValueTask<string> UnPackAsync(int pedestrian)
    {
        _blackBoxState.State.UnboxedBy.Add(pedestrian);
        return ValueTask.FromResult(_blackBoxState.State.Description);
    }

    public ValueTask<CommonStatistics> GetStatisticsAsync()
    {
        return ValueTask.FromResult(new CommonStatistics(_blackBoxState.State.UnboxedBy.Count));
    }
}