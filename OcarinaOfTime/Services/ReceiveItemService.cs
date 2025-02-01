using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Models;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class ReceiveItemService(IMemoryService memoryService, CurrentSceneService currentSceneService)
{
	public async Task<ushort> GetLocalReceivedItemIndex()
	{
		const uint localReceivedItemsCountAddress = 0xA011A660;
		var localReceivedItemsCount = await memoryService.Read16(localReceivedItemsCountAddress);

		return localReceivedItemsCount;
	}

	public async Task ReceiveItem(ItemInfo item)
	{
		const uint incomingPlayerAddress = 0xA0400026;
		const uint incomingItemAddress = 0xA0400028;

		// Verify that the last item was processed already before trying to send another
		var memoryReadCommands = new MemoryReadCommand[]
		{
			new()
			{
				Address = incomingPlayerAddress,
				NumberOfBytes = 2,
			},
			new()
			{
				Address = incomingItemAddress,
				NumberOfBytes = 2,
			},
		};
		var currentIncomingMemoryState = await memoryService.ReadMemoryToLongMulti(memoryReadCommands);
		if (currentIncomingMemoryState[incomingPlayerAddress] != 0 ||
			currentIncomingMemoryState[incomingItemAddress] != 0)
		{
			return;
		}

		await memoryService.Write16(address: incomingPlayerAddress, dataToWrite: 0x00);
		await memoryService.Write16(address: incomingItemAddress, dataToWrite: (ushort)(item.ItemId - 66000));
	}

	public async Task<bool> CanReceiveItem()
	{
		return !ShopScenes.Contains(await currentSceneService.GetCurrentScene());
	}

	private static readonly HashSet<ushort> ShopScenes = [0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x42, 0x4B];
}
