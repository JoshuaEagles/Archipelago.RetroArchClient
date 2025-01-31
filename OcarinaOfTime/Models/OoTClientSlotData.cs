using System.Collections.Generic;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Models;

// Maybe add documentation detailing what this model record is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public record OoTClientSlotData
{
	public required bool ShuffleScrubs { get; init; }
	public required uint CollectibleOverridesFlagsAddress { get; init; }
	public required List<CollectibleFlagOffset> CollectibleFlagOffsets { get; init; }
}
