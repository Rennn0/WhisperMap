using WhisperMap.Contracts;
using WhisperMap.Data;
using WhisperMap.Models;

namespace WhisperMap.Handlers;

public class MapContextUserHandler : IUserHandler
{
    private readonly MapContext _mapContext;

    public MapContextUserHandler(MapContext mapContext)
    {
        _mapContext = mapContext;
    }

    public async Task<Pedestrian> AddUserAsync(PostUserContract contract)
    {
        Pedestrian ped = new Pedestrian { Username = contract.Username };
        await _mapContext.AddAsync(ped);
        await _mapContext.SaveChangesAsync();
        return ped;
    }
}