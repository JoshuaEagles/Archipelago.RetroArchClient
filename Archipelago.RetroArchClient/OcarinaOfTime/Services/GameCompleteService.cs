using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class GameCompleteService(IMemoryService memoryService)
{
    private bool _isGameComplete;

    public async Task<bool> IsGameComplete()
    {
        if (_isGameComplete)
        {
            return true;
        }

        var scenePointerValue = await memoryService.Read32(
            address: AddressConstants.ScenePointerAddress);

        if (scenePointerValue is not
            ((uint)AddressConstants.TriforceHuntCompleteCreditsCutscenePointer
            or (uint)AddressConstants.GanonDefeatedCutscenePointer))
        {
            return false;
        }

        _isGameComplete = true;
        return true;
    }
}
