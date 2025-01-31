using Archipelago.OoTClient.Net.OcarinaOfTime.Enums;
using Archipelago.OoTClient.Net.OcarinaOfTime.Models;
using Archipelago.OoTClient.Net.Services.Interfaces;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class GameModeService(IMemoryService memoryService)
{
	public async Task<GameMode> GetCurrentGameMode()
	{
		var logoState = await GetLogoState();
		
		if (logoState is 0x802C5880 or 0)
		{
			return AvailableGameModes[GameModes.N64Logo];
		}

		var mainState = await GetMainState();
		
		switch (mainState)
		{
			case 1:
				return AvailableGameModes[GameModes.TitleScreen];
			case 2:
				return AvailableGameModes[GameModes.FileSelect];
		}

		var menuState = await GetMenuState();
		
		switch (menuState)
		{
			case 0:
			{
				var isLinkDying = await GetLinkIsDying();
				
				if (isLinkDying)
				{
					return AvailableGameModes[GameModes.Dying];
				}

				var subState = await GetSubState();
				
				return subState == 4 
					? AvailableGameModes[GameModes.Cutscene] 
					: AvailableGameModes[GameModes.NormalGameplay];
			}
			case < 9 or 13 or 18 or 19:
				return AvailableGameModes[GameModes.Paused];
			case 9 or 0xB:
				return AvailableGameModes[GameModes.DyingMenuStart];
			default:
				return AvailableGameModes[GameModes.Dead];
		}
	}

	private async Task<byte> GetMainState()
	{
		const uint mainStateOffset = 0xA011B92F;

		return await memoryService.Read8(mainStateOffset);
	}

	private async Task<byte> GetSubState()
	{
		const uint subStateOffset = 0xA011B933;

		return await memoryService.Read8(subStateOffset);
	}

	private async Task<byte> GetMenuState()
	{
		const uint menuStateOffset = 0xA01D8DD5;

		return await memoryService.Read8(menuStateOffset);
	}

	private async Task<uint> GetLogoState()
	{
		const uint logoStateOffset = 0xA011F200;

		return await memoryService.Read32(logoStateOffset);
	}

	private async Task<bool> GetLinkIsDying()
	{
		const uint linkStateOffset = 0xA01DB09C;
		const uint linkHealthOffset = 0xA011A600;

		var linkState = await memoryService.Read32(linkStateOffset);
		var linkHealth = await memoryService.Read16(linkHealthOffset);

		return (linkState & 0x00000080) > 0 && linkHealth == 0;
	}
	
	private static readonly Dictionary<GameModes, GameMode> AvailableGameModes = new GameMode[]
	{
		new() { CurrentGameMode = GameModes.N64Logo, IsInGame = false },
		new() { CurrentGameMode = GameModes.TitleScreen, IsInGame = false },
		new() { CurrentGameMode = GameModes.FileSelect, IsInGame = false },
		new() { CurrentGameMode = GameModes.Dying, IsInGame = true },
		new() { CurrentGameMode = GameModes.Cutscene, IsInGame = true },
		new() { CurrentGameMode = GameModes.NormalGameplay, IsInGame = true },
		new() { CurrentGameMode = GameModes.Paused, IsInGame = true },
		new() { CurrentGameMode = GameModes.DyingMenuStart, IsInGame = false },
		new() { CurrentGameMode = GameModes.Dead, IsInGame = false },
	}.ToDictionary(gameMode => gameMode.CurrentGameMode);
}
