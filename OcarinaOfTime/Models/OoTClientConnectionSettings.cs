namespace Archipelago.RetroArchClient.OcarinaOfTime.Models;

// Maybe add documentation detailing what this model record is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public record OoTClientConnectionSettings
{
	public required string ArchipelagoHostName { get; init; }
	public required int ArchipelagoPort { get; init; }
	public required string SlotName { get; init; }

	public required string RetroArchHostName { get; init; }
	public required int RetroArchPort { get; init; }
}
