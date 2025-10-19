namespace WhisperMap.Contracts;

public record PostPinContract(
    int PedestrianId,
    double Latitude,
    double Longitude,
    TextBox? TextBox,
    AudioBox? AudioBox
);

public record TextBox(string Text);

public record AudioBox(byte[] Audio);