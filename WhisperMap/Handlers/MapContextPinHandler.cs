using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WhisperMap.Contracts;
using WhisperMap.Data;
using WhisperMap.Models;
using Object = WhisperMap.Models.Object;

namespace WhisperMap.Handlers;

public class MapContextPinHandler : IPinHandler
{
    private readonly MapContext _mapContext;

    public MapContextPinHandler(MapContext mapContext)
    {
        _mapContext = mapContext;
    }

    public async Task<Pin> AddPinAsync(PostPinContract contract)
    {
        Pin pin = new Pin
        {
            Pedestrianid = contract.PedestrianId,
            Location = new Point(contract.Latitude, contract.Longitude) { SRID = 4326 }
        };

        //#TODO decide pin can have multi or single obj attached?? 
        string desc =
            $"{(contract.TextBox is null ? "" : nameof(TextBox))}_{(contract.AudioBox is null ? "" : nameof(AudioBox))}";
        Object pinObj = new Object
        {
            Description = desc
        };

        pin.Object = pinObj;

        await _mapContext.AddAsync(pinObj);
        await _mapContext.AddAsync(pin);
        await _mapContext.SaveChangesAsync();
        return pin;
    }

    public Task<Pin?> GetPinAsync(int pinId)
    {
        return _mapContext.Pins.Include(p => p.Object).Include(p => p.Pedestrian)
            .FirstOrDefaultAsync(p => p.Id == pinId);
    }


    public async Task<IEnumerable<PinWithDistance>> GetNearbyPinsAsync(GetNearbyPinsContract contract)
    {
        Point point = new Point(contract.Latitude, contract.Longitude) { SRID = 4326 };
        return await _mapContext.Pins.AsNoTracking()
            .Where(p => p.Location.IsWithinDistance(point, contract.Radius))
            .Include(p => p.Object)
            .Include(p => p.Pedestrian).Select(p => new PinWithDistance(
                p,
                EF.Functions.Distance(p.Location, point, true)
            )).ToListAsync();
    }
}