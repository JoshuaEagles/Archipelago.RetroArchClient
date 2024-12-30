using Archipelago.MultiClient.Net.Models;
using OOT_AP_Client.Models;
using OOT_AP_Client.Services.Interfaces;

namespace OOT_AP_Client.OcarinaOfTime.Services;

public class ReceiveItemService
{
	private readonly CurrentSceneService _currentSceneService;
	private readonly IMemoryService _memoryService;

	public ReceiveItemService(IMemoryService memoryService, CurrentSceneService currentSceneService)
	{
		_memoryService = memoryService;
		_currentSceneService = currentSceneService;
	}

	public async Task<ushort> GetLocalReceivedItemIndex()
	{
		const uint localReceivedItemsCountAddress = 0xA011A660;
		var localReceivedItemsCount = await _memoryService.Read16(localReceivedItemsCountAddress);

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
		var currentIncomingMemoryState = await _memoryService.ReadMemoryToLongMulti(memoryReadCommands);
		if (currentIncomingMemoryState[incomingPlayerAddress] != 0 ||
			currentIncomingMemoryState[incomingItemAddress] != 0)
		{
			return;
		}

		await _memoryService.Write16(address: incomingPlayerAddress, dataToWrite: 0x00);
		await _memoryService.Write16(address: incomingItemAddress, dataToWrite: (ushort)(item.ItemId - 66000));
	}

	public async Task<bool> CanReceiveItem()
	{
		return !ShopScenes.Contains(await _currentSceneService.GetCurrentScene());
	}

	private static readonly HashSet<ushort> ShopScenes = [0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x42, 0x4B];
}
