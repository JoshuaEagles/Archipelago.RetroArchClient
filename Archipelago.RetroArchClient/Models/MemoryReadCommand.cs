namespace Archipelago.RetroArchClient.Models;

// Maybe add documentation detailing what this model record is for, what it represents, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See OcarinaOfTime.Enums.GameModes for example on how this could be achieved.
public record MemoryReadCommand
{
	public required long Address { get; init; }
	public required int NumberOfBytes { get; init; }
}