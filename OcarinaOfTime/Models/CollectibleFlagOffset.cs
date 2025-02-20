namespace Archipelago.RetroArchClient.OcarinaOfTime.Models;

// Maybe add documentation detailing what this model record is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public record CollectibleFlagOffset
{
	public required long ItemId { get; init; }
	public required long Offset { get; init; }
	public required long Flag { get; init; }
}