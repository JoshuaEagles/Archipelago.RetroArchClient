using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.Services.Interfaces;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

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
				GetAddressForCollectibleOffset(
					collectibleOverridesFlagAddress: collectibleOverridesFlagAddress, 
					collectibleFlagOffset: collectibleFlagOffset);

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

		var memoryDictionary = await memoryService.ReadMemoryToLongMulti(readCommands: memoryReadCommands);

		return
			collectibleFlagOffsets
				.Select(collectibleFlagOffset => new
				{
					collectibleFlagOffset,
					addressOfTargetByte =
						GetAddressForCollectibleOffset(
							collectibleOverridesFlagAddress: collectibleOverridesFlagAddress,
							collectibleFlagOffset: collectibleFlagOffset)
				})
				.Select(x => new
				{
					x.collectibleFlagOffset,
					memoryContainingFlag = memoryDictionary[x.addressOfTargetByte]
				})
				.Where(x => ByteUtils.CheckBit(
					memoryToCheck: x.memoryContainingFlag, 
					bitToCheck: (byte)(x.collectibleFlagOffset.Flag % 8)))
				.Select(x => x.collectibleFlagOffset.ItemId)
				.ToList();
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
