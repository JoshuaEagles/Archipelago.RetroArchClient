using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

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
        => await memoryService.Read8(
            address: AddressConstants.MainStateOffset);

    private async Task<byte> GetSubState()
        => await memoryService.Read8(
            address: AddressConstants.SubStateOffset);

    private async Task<byte> GetMenuState()
        => await memoryService.Read8(
            address: AddressConstants.MenuStateOffset);

    private async Task<uint> GetLogoState()
        => await memoryService.Read32(
            address: AddressConstants.LogoStateOffset);

    private async Task<bool> GetLinkIsDying()
    {
        var linkState = await memoryService.Read32(
            address: AddressConstants.LinkStateOffset);
        var linkHealth = await memoryService.Read16(
            address: AddressConstants.LinkHealthOffset);

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
