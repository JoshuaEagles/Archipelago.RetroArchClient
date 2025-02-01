using System.Threading.Tasks;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class GameCompleteService(IMemoryService memoryService)
{
	private bool _isGameComplete;

	public async Task<bool> IsGameComplete()
	{
		if (_isGameComplete)
		{
			return true;
		}

		const uint scenePointerAddress = 0xA01CA208;
		var scenePointerValue = await memoryService.Read32(scenePointerAddress);

		const uint triforceHuntCompleteCreditsCutscenePointer = 0x80383C10;
		const uint ganonDefeatedCutscenePointer = 0x80382720;

		if (scenePointerValue is not (triforceHuntCompleteCreditsCutscenePointer or ganonDefeatedCutscenePointer))
		{
			return false;
		}

		_isGameComplete = true;
		return true;
	}
}
