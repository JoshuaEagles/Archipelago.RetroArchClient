using Archipelago.OoTClient.Net.OcarinaOfTime.Enums;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Models;

// Maybe add documentation detailing what this model record is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public record GameMode
{
	public required GameModes CurrentGameMode { get; init; }
	public required bool IsInGame { get; init; }
}
