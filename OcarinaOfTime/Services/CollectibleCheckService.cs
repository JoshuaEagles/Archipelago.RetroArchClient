using Archipelago.OoTClient.Net.Models;
using Archipelago.OoTClient.Net.OcarinaOfTime.Models;
using Archipelago.OoTClient.Net.Services.Interfaces;
using Archipelago.OoTClient.Net.Utils;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class CollectibleCheckService(IMemoryService memoryService)
{
	public async Task<List<long>> GetAllCheckedCollectibleIds(
		long collectibleOverridesFlagAddress,
		List<CollectibleFlagOffset> collectibleFlagOffsets
	)
	{
		var memoryReadCommands = new List<MemoryReadCommand>();
		var alreadyQueuedOffsets = new HashSet<long>();
		
		foreach (var collectibleFlagOffset in collectibleFlagOffsets)
		{
			var addressOfTargetByte =
				GetAddressForCollectibleOffset(collectibleOverridesFlagAddress, collectibleFlagOffset);

			if (alreadyQueuedOffsets.Contains(addressOfTargetByte))
			{
				continue;
			}

			var memoryReadCommand = new MemoryReadCommand
			{
				Address = addressOfTargetByte,
				NumberOfBytes = 1,
			};
			
			memoryReadCommands.Add(memoryReadCommand);
			alreadyQueuedOffsets.Add(addressOfTargetByte);
		}

		var memoryDictionary = await memoryService.ReadMemoryToLongMulti(memoryReadCommands);

		return (from collectibleFlagOffset in collectibleFlagOffsets
			let addressOfTargetByte =
				GetAddressForCollectibleOffset(collectibleOverridesFlagAddress, collectibleFlagOffset)
			let memoryContainingFlag = memoryDictionary[addressOfTargetByte]
			where ByteUtils.CheckBit(memoryContainingFlag, (byte)(collectibleFlagOffset.Flag % 8))
			select collectibleFlagOffset.ItemId).ToList();
	}

	private static long GetAddressForCollectibleOffset(
		long collectibleOverridesFlagAddress,
		CollectibleFlagOffset collectibleFlagOffset
	)
	{
		// eg 0 to 7 gives 0, 8 to 15 gives 1, index to which byte contains the bit we want
		var byteContainingTargetBit = collectibleFlagOffset.Flag >> 3;
		var addressOfTargetByte = collectibleOverridesFlagAddress
			+ collectibleFlagOffset.Offset
			+ byteContainingTargetBit;

		return addressOfTargetByte;
	}
}
