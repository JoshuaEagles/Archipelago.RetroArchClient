using Archipelago.RetroArchClient.OcarinaOfTime.Enums;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models;

// Maybe add documentation detailing what this model class is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class LocationInformation(string name, LocationType type, byte offset, byte bitToCheck, Area area)
{
	public string Name { get; } = name;
	public LocationType Type { get; } = type;
	public byte Offset { get; } = offset;
	public byte BitToCheck { get; } = bitToCheck;
	public Area Area { get; } = area;
}
