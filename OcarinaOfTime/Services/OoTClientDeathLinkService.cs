using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class OoTClientDeathLinkService(
	IMemoryService memoryService,
	GameModeService gameModeService,
	CurrentSceneService currentSceneService)
{
	public bool DeathLinkEnabled { get; private set; }

	private bool _hasDied;
	private bool _receivedDeathLinkQueued;
	private bool _deathLinkSent;

	public async Task StoreDeathLinkEnabledFromMemory()
	{
		const uint deathLinkEnabledFlagAddress = 0xA040002B;

		var deathLinkEnabledFlag = await memoryService.Read8(deathLinkEnabledFlagAddress);

		DeathLinkEnabled = deathLinkEnabledFlag > 0;
	}

	/// <summary>
	/// Returns immediately if death link is not enabled.
	/// If death link is enabled, handles the logic for killing Link if a death link is queued,
	/// and sending out death link when link dies.
	/// </summary>
	/// <returns>Bool of whether to send a death link out</returns>
	public async Task<bool> ProcessDeathLink()
	{
		if (!DeathLinkEnabled)
		{
			return false;
		}

		var currentGameMode = await gameModeService.GetCurrentGameMode();

		if (!_hasDied && currentGameMode.CurrentGameMode == GameModes.Dying)
		{
			_hasDied = true;
		}

		if (_hasDied && currentGameMode.CurrentGameMode == GameModes.NormalGameplay)
		{
			_receivedDeathLinkQueued = false;
			_deathLinkSent = false;
			_hasDied = false;
		}

		if (!_receivedDeathLinkQueued || _hasDied || currentGameMode.CurrentGameMode != GameModes.NormalGameplay)
		{
			return ShouldSendDeathLink();
		}
		
		var currentScene = await currentSceneService.GetCurrentScene();

		if (DeathCrashScenes.Contains(currentScene))
		{
			return false;
		}

		const uint linkHealthAddress = 0xA011A600;
		await memoryService.Write16(address: linkHealthAddress, dataToWrite: 0);

		return ShouldSendDeathLink();
	}

	public void ReceiveDeathLink()
	{
		_receivedDeathLinkQueued = true;
	}

	private bool ShouldSendDeathLink()
	{
		if (_deathLinkSent)
		{
			return false;
		}

		if (_receivedDeathLinkQueued || !_hasDied)
		{
			return false;
		}
		
		_deathLinkSent = true;
		return true;

	}

	// The game crashes if killed in the market entrance or outside the ToT
	// -- Is this intended? Or is this a quirk with RetroArch and the core?
	private static readonly HashSet<ushort> DeathCrashScenes = [27, 28, 29, 35, 36, 37];
}
