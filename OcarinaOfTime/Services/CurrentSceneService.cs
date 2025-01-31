using Archipelago.OoTClient.Net.Services.Interfaces;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class CurrentSceneService(IMemoryService memoryService)
{
	public async Task<ushort> GetCurrentScene()
	{
		const uint currentSceneAddress = 0xA01C8544;

		return await memoryService.Read16(currentSceneAddress);
	}
}
