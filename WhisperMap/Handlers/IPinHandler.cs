using WhisperMap.Contracts;
using WhisperMap.Models;

namespace WhisperMap.Handlers;

public interface IPinHandler
{
    Task<Pin> AddPinAsync(PostPinContract contract);
    Task<Pin?> GetPinAsync(int pinId);
    Task<IEnumerable<PinWithDistance>> GetNearbyPinsAsync(GetNearbyPinsContract contract);
}