namespace WhisperMap.Models;

[GenerateSerializer]
[Alias("BlackBoxState")]
public class BlackBoxState
{
    [Id(0)] public int BoxOwner { get; set; }

    [Id(1)] public int PinId { get; set; }

    [Id(4)] public string Description { get; set; } = string.Empty;

    [Id(3)] public List<int> UnboxedBy { get; } = [];
}

[GenerateSerializer]
[Alias("AudioBoxState")]
public class AudioBoxState : BlackBoxState
{
    [Id(0)] public byte[] Audio { get; set; } = [];
}

[GenerateSerializer]
[Alias("CommonStatistics")]
public record CommonStatistics(int UnboxCount);