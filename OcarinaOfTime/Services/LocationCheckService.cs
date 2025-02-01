using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.Services.Interfaces;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class LocationCheckService(IMemoryService memoryService, GameModeService gameModeService)
{
	private readonly HashSet<Area> _areasToSkipChecking = [];

	private int _bigPoePointsRequired = int.MaxValue;

	public async Task InitializeMasterQuestHandling()
	{
		var autoTrackerContextAddress = await memoryService.Read32(0xA040000C) - 0x80000000 + 0xA0000000;
		var masterQuestTableAddress = autoTrackerContextAddress + 0x02E;

		var dungeonToDungeonId = new Dictionary<Area, byte>
		{
			{ Area.DekuTree, 0x0 },
			{ Area.DodongosCavern, 0x1 },
			{ Area.JabuJabusBelly, 0x2 },
			{ Area.ForestTemple, 0x3 },
			{ Area.FireTemple, 0x4 },
			{ Area.WaterTemple, 0x5 },
			{ Area.SpiritTemple, 0x6 },
			{ Area.ShadowTemple, 0x7 },
			{ Area.BottomOfTheWell, 0x8 },
			{ Area.IceCavern, 0x9 },
			{ Area.GerudoTrainingGround, 0xB },
			{ Area.GanonsCastle, 0xD },
		};

		var memoryReadCommands = dungeonToDungeonId.Values.Select(dungeonId => new MemoryReadCommand
			{ Address = masterQuestTableAddress + dungeonId, NumberOfBytes = 1 }).ToList();
		
		var memoryDictionary = await memoryService.ReadMemoryToLongMulti(memoryReadCommands);

		foreach (var (area, dungeonId) in dungeonToDungeonId)
		{
			// Currently this takes advantage of the fact that the MQ versions of each dungeon are right after the regular one in the enum
			// Don't really like this though, should change it at some point
			var isMasterQuest = memoryDictionary[masterQuestTableAddress + dungeonId] == 1;
			var areaToSkip = isMasterQuest ? area : area + 1;

			_areasToSkipChecking.Add(areaToSkip);
		}
	}

	public async Task InitializeBigPoesRequired()
	{
		const long bigPoesRequiredAddress = 0xA0400EAD;
		var bigPoesRequired = await memoryService.Read8(bigPoesRequiredAddress);
		_bigPoePointsRequired = bigPoesRequired * 100;
	}

	// Going to want some sort of caching system for this so that it doesn't keep reporting every single location every single time
	// Might be as simple as a hashmap somewhere that gets loaded with all received items, and stuff only gets sent to the server when it's not in that hashmap
	public async Task<List<string>> GetAllCheckedLocationNames(OoTClientSlotData ootClientSlotData)
	{
		var outgoingItemKey
			= await memoryService.ReadMemoryToByteArray(0x8040002c, 4);

		// Since this is async with the emulator, there's a chance that the key
		// gets populated after we read it but before we write to clear it
		// So we make sure we actually read some data before clearing it
		if (outgoingItemKey.Any(b => b != 0x00))
		{
			await memoryService.WriteByteArray(0x8040002c, "\0\0\0\0"u8.ToArray());
			await memoryService.WriteByteArray(0x80400030, "\0\0\0\0"u8.ToArray());
		}

		var locationsToCheck
			= AllLocationInformation
				.AllLocations
				.Where(location => !_areasToSkipChecking.Contains(location.Area))
				// Only include scrubsanity checks if it's enabled
				.Where(location => location.Type != LocationType.Scrubsanity || ootClientSlotData.ShuffleScrubs)
				.ToImmutableArray();

		var checkedMemoryAddresses = new HashSet<long>();
		var memoryReadCommands =
			locationsToCheck.Select(GetMemoryReadCommandForLocation)
				.Where(memoryReadCommand => checkedMemoryAddresses.Add(memoryReadCommand.Address)).ToList();

		var memoryDictionary = await memoryService.ReadMemoryToLongMulti(memoryReadCommands);

		// make sure the game wasn't reset before using the memory that was read
		var currentGameMode = await gameModeService.GetCurrentGameMode();
		if (!currentGameMode.IsInGame)
		{
			return [];
		}

		// Iterate over the locationsToCheck again, using the dictionary for memory checks now
		// 31st Jan 2025: Converted to LINQ expression
		return (from locationInformation in locationsToCheck
			where CheckLocation(locationInformation, outgoingItemKey, memoryDictionary)
			select locationInformation.Name).ToList();
	}

	private static MemoryReadCommand GetMemoryReadCommandForLocation(LocationInformation locationInformation)
	{
		return locationInformation.Type switch
		{
			LocationType.Chest or LocationType.FireArrows => GetChestFlagsReadCommand(locationInformation),
			LocationType.GroundItem or LocationType.BossItem or LocationType.Cow or LocationType.Medigoron
				or LocationType.BeanSale
				or LocationType.BombchuSalesman => GetCollectibleFlagsReadCommand(locationInformation),
			LocationType.GreatFairy or LocationType.TrailGreatFairy or LocationType.CraterGreatFairy =>
				GetGreatFairyReadCommand(locationInformation),
			LocationType.Scrubsanity => GetScrubsanityFlagsReadCommand(locationInformation),
			LocationType.Skulltula => GetSkulltulaReadCommand(locationInformation),
			LocationType.Shop => GetShopReadCommand(),
			LocationType.Event => GetEventReadCommand(locationInformation),
			LocationType.FishingChild or LocationType.FishingAdult => GetFishingReadCommand(),
			LocationType.GetInfo => GetGetInfoReadCommand(locationInformation),
			LocationType.InfoTable => GetInfoTableReadCommand(locationInformation),
			LocationType.BiggoronSword => GetBiggoronSwordReadCommand(),
			LocationType.MembershipCardCheck => GetMembershipCardReadCommand(),
			LocationType.BigPoeBottle => GetBigPoeReadCommand(),
			_ => throw new InvalidOperationException(
				$"Unknown LocationType {locationInformation.Type} for location {locationInformation.Name}")
		};
	}

	// This is also where the flag for Fire Arrows is
	private static MemoryReadCommand GetChestFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(locationInformation.Offset, 0x0);

	// Applies to all three types of Great Fairy check
	private static MemoryReadCommand GetGreatFairyReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(locationInformation.Offset, 0x4);

	private static MemoryReadCommand GetCollectibleFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(locationInformation.Offset, 0xC);

	private static MemoryReadCommand GetScrubsanityFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(locationInformation.Offset, 0x10);

	private static MemoryReadCommand GetSkulltulaReadCommand(LocationInformation locationInformation)
	{
		var skulltulaLocationAddress = GetSkulltulaLocationAddress(locationInformation);

		return new MemoryReadCommand
		{
			Address = skulltulaLocationAddress,
			NumberOfBytes = 1,
		};
	}

	private static MemoryReadCommand GetShopReadCommand()
	{
		// TODO: Should centralize and de-duplicate these offset constants
		const long shopContextOffset = 0xA011AB84;

		return new MemoryReadCommand { Address = shopContextOffset, NumberOfBytes = 4 };
	}

	private static MemoryReadCommand GetBigPoeReadCommand()
	{
		const long bigPoesPointsAddress = 0xA011B48C;

		return new MemoryReadCommand { Address = bigPoesPointsAddress, NumberOfBytes = 4 };
	}

	private static MemoryReadCommand GetEventReadCommand(LocationInformation locationInformation)
	{
		const long eventContextAddress = 0xA011B4A4;

		var eventAddress = eventContextAddress + locationInformation.Offset * 2;

		return new MemoryReadCommand { Address = eventAddress, NumberOfBytes = 2 };
	}

	private static MemoryReadCommand GetGetInfoReadCommand(LocationInformation locationInformation)
	{
		const long getInfoStartAddress = 0xA011B4C0;

		var itemGetInfoAddress = getInfoStartAddress + locationInformation.Offset;

		return new MemoryReadCommand { Address = itemGetInfoAddress, NumberOfBytes = 1 };
	}

	private static MemoryReadCommand GetInfoTableReadCommand(LocationInformation locationInformation)
	{
		const long infoTableStartAddress = 0xA011B4C8;

		var itemInfoTableAddress = infoTableStartAddress + locationInformation.Offset;

		return new MemoryReadCommand { Address = itemInfoTableAddress, NumberOfBytes = 1 };
	}

	private static MemoryReadCommand GetMembershipCardReadCommand()
	{
		const long eventContextAddress = 0xA011B4A4;
		const long eventAddress = eventContextAddress + 0x9 * 2;

		return new MemoryReadCommand { Address = eventAddress, NumberOfBytes = 2 };
	}

	private static MemoryReadCommand GetFishingReadCommand()
	{
		const long fishingContextAddress = 0xA011B490;

		return new MemoryReadCommand { Address = fishingContextAddress, NumberOfBytes = 4 };
	}

	private static MemoryReadCommand GetBiggoronSwordReadCommand()
	{
		const long equipmentOffset = 0xA011A640;

		return new MemoryReadCommand { Address = equipmentOffset, NumberOfBytes = 4 };
	}

	private bool CheckLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
	{
		return locationInformation.Type switch
		{
			LocationType.Chest => CheckChestLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.Cow => CheckCowLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.Skulltula => CheckSkulltulaLocation(locationInformation, memoryDictionary),
			LocationType.Shop => CheckShopLocation(locationInformation, memoryDictionary),
			LocationType.GroundItem => CheckGroundItemLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.Event => CheckEventLocation(locationInformation, memoryDictionary),
			LocationType.GetInfo => CheckGetInfoLocation(locationInformation, memoryDictionary),
			LocationType.InfoTable => CheckInfoTableLocation(locationInformation, memoryDictionary),
			LocationType.Scrubsanity => CheckScrubsanityLocation(locationInformation, memoryDictionary),
			LocationType.BossItem => CheckBossItemLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.BigPoeBottle => CheckBigPoeBottleLocation(memoryDictionary),
			LocationType.GreatFairy => CheckGreatFairyMagicLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.TrailGreatFairy => CheckTrailGreatFairyMagicLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.CraterGreatFairy => CheckCraterGreatFairyMagicLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.Medigoron => CheckMedigoronLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.BiggoronSword => CheckBiggoronSwordLocation(memoryDictionary),
			LocationType.BeanSale => CheckBeanSaleLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.FishingChild => CheckFishingLocation(false, memoryDictionary),
			LocationType.FishingAdult => CheckFishingLocation(true, memoryDictionary),
			LocationType.FireArrows => CheckFireArrowsLocation(locationInformation, outgoingItemKey, memoryDictionary),
			LocationType.MembershipCardCheck => CheckMembershipCardLocation(memoryDictionary),
			LocationType.BombchuSalesman => CheckBombchuSalesmanLocation(locationInformation, outgoingItemKey, memoryDictionary),
			_ => throw new InvalidOperationException(
				$"Unknown LocationType {locationInformation.Type} for location {locationInformation.Name}"
			),
		};
	}

	private static bool CheckChestLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, locationInformation.BitToCheck, 0x1) 
		       || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x0, memoryDictionary);

	private static bool CheckGroundItemLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, locationInformation.BitToCheck, 0x2)
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0xC, memoryDictionary);

	private static bool CheckBossItemLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, 0x4F, 0x0) 
		       || SceneCheck(locationInformation.Offset, 0x1F, 0xC, memoryDictionary);

	// updates save context immediately, so the existing client doesn't check temp context
	// need to figure out the temp context format eventually so that we can do temp context more efficiently later
	private static bool CheckScrubsanityLocation(LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary)
		=> SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x10, memoryDictionary);

	private static bool CheckCowLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, (byte)(locationInformation.BitToCheck - 0x03), 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0xC, memoryDictionary);

	private static bool CheckGreatFairyMagicLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, locationInformation.BitToCheck, 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x04, memoryDictionary);

	private static bool CheckTrailGreatFairyMagicLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, 0xFF, 0x13, 0x05) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x04, memoryDictionary);

	private static bool CheckCraterGreatFairyMagicLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, 0xFF, 0x14, 0x05) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x04, memoryDictionary);

	private static bool CheckMedigoronLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, 0x16, 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0xC, memoryDictionary);

	private static bool CheckBeanSaleLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, 0x16, 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0xC, memoryDictionary);
	
	private static bool CheckBombchuSalesmanLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, 0x03, 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0xC, memoryDictionary);

	private static bool CheckSkulltulaLocation(LocationInformation locationInformation, 
		Dictionary<long, long> memoryDictionary)
	{
		var skulltulaLocationAddress = GetSkulltulaLocationAddress(locationInformation);
		var nearbyMemory = memoryDictionary[skulltulaLocationAddress];

		return ByteUtils.CheckBit(memoryToCheck: nearbyMemory, bitToCheck: locationInformation.BitToCheck);
	}

	// TODO: need to document and figure this out more
	private static long GetSkulltulaLocationAddress(LocationInformation locationInformation)
	{
		const long skulltulaFlagsOffset = 0xA011B46C;

		var skulltulaArrayIndex = locationInformation.Offset + 3 - 2 * (locationInformation.Offset % 4);
		var localSkulltulaOffset = skulltulaFlagsOffset + skulltulaArrayIndex;

		return localSkulltulaOffset;
	}

	private static bool CheckShopLocation(LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary)
	{
		const long shopContextOffset = 0xA011AB84;

		var nearbyMemory = memoryDictionary[shopContextOffset];
		var bitToCheck = locationInformation.Offset * 4 + locationInformation.BitToCheck;

		return ByteUtils.CheckBit(nearbyMemory, (byte)bitToCheck);
	}

	// Checked via looking at the points on the card, rather than getting the item itself
	private bool CheckBigPoeBottleLocation(Dictionary<long, long> memoryDictionary)
	{
		const long bigPoesPointsAddress = 0xA011B48C;
		var bigPoesPoints = memoryDictionary[bigPoesPointsAddress];

		return bigPoesPoints >= _bigPoePointsRequired;
	}

	private static bool CheckEventLocation(LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary)
	{
		const long eventContextAddress = 0xA011B4A4;

		var eventAddress = eventContextAddress + locationInformation.Offset * 2;
		var eventRow = memoryDictionary[eventAddress];

		return ByteUtils.CheckBit(eventRow, locationInformation.BitToCheck);
	}

	private static bool CheckGetInfoLocation(LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary)
	{
		const long getInfoStartAddress = 0xA011B4C0;

		var itemGetInfoAddress = getInfoStartAddress + locationInformation.Offset;
		var nearbyMemory = memoryDictionary[itemGetInfoAddress];

		return ByteUtils.CheckBit(nearbyMemory, locationInformation.BitToCheck);
	}

	private static bool CheckInfoTableLocation(LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary)
	{
		const long infoTableStartAddress = 0xA011B4C8;

		var itemInfoTableAddress = infoTableStartAddress + locationInformation.Offset;
		var nearbyMemory = memoryDictionary[itemInfoTableAddress];

		return ByteUtils.CheckBit(nearbyMemory, locationInformation.BitToCheck);
	}

	private static bool CheckMembershipCardLocation(Dictionary<long, long> memoryDictionary)
	{
		const long eventContextAddress = 0xA011B4A4;
		const long eventAddress = eventContextAddress + 0x9 * 2;

		var eventRow = memoryDictionary[eventAddress];

		return ByteUtils.CheckBit(eventRow, 0)
			&& ByteUtils.CheckBit(eventRow, 1)
			&& ByteUtils.CheckBit(eventRow, 2)
			&& ByteUtils.CheckBit(eventRow, 3);
	}

	private static bool CheckFishingLocation(bool isAdult, Dictionary<long, long> memoryDictionary)
	{
		const long fishingContextAddress = 0xA011B490;

		var bitToCheck = isAdult ? 11 : 10;
		var nearbyMemory = memoryDictionary[fishingContextAddress];

		return ByteUtils.CheckBit(nearbyMemory, (byte)bitToCheck);
	}

	private static bool CheckFireArrowsLocation(LocationInformation locationInformation, byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary)
		=> OutgoingKeyCheck(outgoingItemKey, locationInformation.Offset, 0x58, 0x0) 
		   || SceneCheck(locationInformation.Offset, locationInformation.BitToCheck, 0x0, memoryDictionary);

	private static bool CheckBiggoronSwordLocation(Dictionary<long, long> memoryDictionary)
	{
		const long equipmentOffset = 0xA011A640;
		const byte bitToCheck = 8;

		var nearbyMemory = memoryDictionary[equipmentOffset];

		return ByteUtils.CheckBit(nearbyMemory, bitToCheck);
	}

	private static bool SceneCheck(byte sceneOffset, byte bitToCheck, byte sceneDataOffset,
		Dictionary<long, long> memoryDictionary)
	{
		var localSceneOffset = GetLocalSceneOffset(sceneOffset: sceneOffset, sceneDataOffset: sceneDataOffset);
		var nearbyMemory = memoryDictionary[localSceneOffset];

		return ByteUtils.CheckBit(nearbyMemory, bitToCheck);
	}

	private static MemoryReadCommand GetLocalSceneMemoryReadCommand(byte sceneOffset, byte sceneDataOffset)
	{
		var localSceneOffset = GetLocalSceneOffset(sceneOffset, sceneDataOffset);

		return new MemoryReadCommand
		{
			Address = localSceneOffset,
			NumberOfBytes = 4,
		};
	}

	private static long GetLocalSceneOffset(byte sceneOffset, byte sceneDataOffset)
	{
		const long sceneFlagsOffset = 0xA011A6A4;

		var localSceneOffset = sceneFlagsOffset + 0x1c * sceneOffset + sceneDataOffset;

		return localSceneOffset;
	}

	private static bool OutgoingKeyCheck(byte[] outgoingItemKey, byte sceneOffset, byte bitToCheck,
		byte ootrLocationType)
	{
		return outgoingItemKey[0] == sceneOffset 
		       && outgoingItemKey[1] == ootrLocationType 
		       && outgoingItemKey[3] == bitToCheck;
	}
}
