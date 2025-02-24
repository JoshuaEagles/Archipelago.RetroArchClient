using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class CurrentSceneService(IMemoryService memoryService)
{
    public async Task<ushort> GetCurrentScene()
        => await memoryService.Read16(
            address: AddressConstants.CurrentSceneAddress);
}
