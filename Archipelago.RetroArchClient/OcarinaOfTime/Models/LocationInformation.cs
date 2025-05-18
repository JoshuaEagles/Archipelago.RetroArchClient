using Archipelago.RetroArchClient.OcarinaOfTime.Enums;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models;

public class LocationInformation(string name, LocationType type, byte offset, byte bitToCheck, Area area)
{
    public string Name { get; } = name;
    public LocationType Type { get; } = type;
    public byte Offset { get; } = offset;
    public byte BitToCheck { get; } = bitToCheck;
    public Area Area { get; } = area;
}
