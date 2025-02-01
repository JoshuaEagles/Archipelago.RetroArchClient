using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Models;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class ReceiveItemService(IMemoryService memoryService, CurrentSceneService currentSceneService)
{
	public async Task<ushort> GetLocalReceivedItemIndex()
	{
		var localReceivedItemsCount = await memoryService.Read16(
			address: AddressConstants.LocalReceivedItemsCountAddress);

		return localReceivedItemsCount;
	}

	public async Task ReceiveItem(ItemInfo item)
	{// Verify that the last item was processed already before trying to send another
		var memoryReadCommands = new MemoryReadCommand[]
		{
			new()
			{
				Address = AddressConstants.IncomingPlayerAddress,
				NumberOfBytes = 2,
			},
			new()
			{
				Address = AddressConstants.IncomingItemAddress,
				NumberOfBytes = 2,
			}
		};
		
		var currentIncomingMemoryState = await memoryService.ReadMemoryToLongMulti(
			readCommands: memoryReadCommands);
		
		if (currentIncomingMemoryState[AddressConstants.IncomingPlayerAddress] != 0 ||
			currentIncomingMemoryState[AddressConstants.IncomingItemAddress] != 0)
		{
			return;
		}

		await memoryService.Write16(
			address: AddressConstants.IncomingPlayerAddress, 
			dataToWrite: 0x00);
		await memoryService.Write16(
			address: AddressConstants.IncomingItemAddress, 
			dataToWrite: (ushort)(item.ItemId - 66000));
	}

	public async Task<bool> CanReceiveItem()
	{
		return !ShopScenes.Contains(await currentSceneService.GetCurrentScene());
	}

	private static readonly HashSet<ushort> ShopScenes = [0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x42, 0x4B];
}
