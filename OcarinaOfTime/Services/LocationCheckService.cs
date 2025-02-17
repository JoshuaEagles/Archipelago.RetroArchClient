using System.Collections.Immutable;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.Services.Interfaces;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class LocationCheckService(IMemoryService memoryService, GameModeService gameModeService)
{
	private readonly HashSet<Area> _areasToSkipChecking = [];

	private int _bigPoePointsRequired = int.MaxValue;

	public async Task InitializeMasterQuestHandling()
	{
		var autoTrackerContextAddress = await memoryService.Read32(
			address: 0xA040000C) - 0x80000000 + 0xA0000000;
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

		var memoryReadCommands = dungeonToDungeonId.Values
			.Select(dungeonId => new MemoryReadCommand
			{
				Address = masterQuestTableAddress + dungeonId,
				NumberOfBytes = 1,
			}).ToList();

		var memoryDictionary = await memoryService.ReadMemoryToLongMulti(
			readCommands: memoryReadCommands);

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
		var bigPoesRequired = await memoryService.Read8(
			address: AddressConstants.BigPoesRequiredAddress);
		_bigPoePointsRequired = bigPoesRequired * 100;
	}

	// Going to want some sort of caching system for this so that it doesn't keep reporting every single location every single time
	// Might be as simple as a hashmap somewhere that gets loaded with all received items, and stuff only gets sent to the server when it's not in that hashmap
	public async Task<List<string>> GetAllCheckedLocationNames(OoTClientSlotData ootClientSlotData)
	{
		var outgoingItemKey
			= await memoryService.ReadMemoryToByteArray(
				address: 0x8040002c,
				numberOfBytes: 4);

		// Since this is async with the emulator, there's a chance that the key
		// gets populated after we read it but before we write to clear it
		// So we make sure we actually read some data before clearing it
		if (outgoingItemKey.Any(b => b != 0x00))
		{
			await memoryService.WriteByteArray(
				address: 0x8040002c,
				dataToWrite: "\0\0\0\0"u8.ToArray());
			await memoryService.WriteByteArray(
				address: 0x80400030,
				dataToWrite: "\0\0\0\0"u8.ToArray());
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
		return locationsToCheck
			.Where(locationInformation => CheckLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary))
			.Select(locationInformation => locationInformation.Name)
			.ToList();
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
				$"Unknown LocationType {locationInformation.Type} for location {locationInformation.Name}"),
		};
	}

	// This is also where the flag for Fire Arrows is
	private static MemoryReadCommand GetChestFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(
			sceneOffset: locationInformation.Offset,
			sceneDataOffset: 0x0);

	// Applies to all three types of Great Fairy check
	private static MemoryReadCommand GetGreatFairyReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(
			sceneOffset: locationInformation.Offset,
			sceneDataOffset: 0x4);

	private static MemoryReadCommand GetCollectibleFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(
			sceneOffset: locationInformation.Offset,
			sceneDataOffset: 0xC);

	private static MemoryReadCommand GetScrubsanityFlagsReadCommand(LocationInformation locationInformation)
		=> GetLocalSceneMemoryReadCommand(
			sceneOffset: locationInformation.Offset,
			sceneDataOffset: 0x10);

	private static MemoryReadCommand GetSkulltulaReadCommand(LocationInformation locationInformation)
	{
		var skulltulaLocationAddress = GetSkulltulaLocationAddress(
			locationInformation: locationInformation);

		return new MemoryReadCommand
		{
			Address = skulltulaLocationAddress,
			NumberOfBytes = 1,
		};
	}

	private static MemoryReadCommand GetShopReadCommand() =>
		new() { Address = AddressConstants.ShopContextOffset, NumberOfBytes = 4 };

	private static MemoryReadCommand GetBigPoeReadCommand() =>
		new() { Address = AddressConstants.BigPoePointsAddress, NumberOfBytes = 4 };


	private static MemoryReadCommand GetEventReadCommand(LocationInformation locationInformation)
	{
		var eventAddress = AddressConstants.EventContextAddress + locationInformation.Offset * 2;

		return new MemoryReadCommand { Address = eventAddress, NumberOfBytes = 2 };
	}

	private static MemoryReadCommand GetGetInfoReadCommand(LocationInformation locationInformation)
	{
		var itemGetInfoAddress = AddressConstants.GetInfoStartAddress + locationInformation.Offset;

		return new MemoryReadCommand { Address = itemGetInfoAddress, NumberOfBytes = 1 };
	}

	private static MemoryReadCommand GetInfoTableReadCommand(LocationInformation locationInformation)
	{
		var itemInfoTableAddress = AddressConstants.InfoTableStartAddress + locationInformation.Offset;

		return new MemoryReadCommand { Address = itemInfoTableAddress, NumberOfBytes = 1 };
	}

	private static MemoryReadCommand GetMembershipCardReadCommand()
	{
		const long eventAddress = AddressConstants.EventContextAddress + 0x9 * 2;

		return new MemoryReadCommand { Address = eventAddress, NumberOfBytes = 2 };
	}


	private static MemoryReadCommand GetFishingReadCommand() =>
		new() { Address = AddressConstants.FishingContextAddress, NumberOfBytes = 4 };

	private static MemoryReadCommand GetBiggoronSwordReadCommand() =>
		new() { Address = AddressConstants.EquipmentOffset, NumberOfBytes = 4 };

	private bool CheckLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
	{
		return locationInformation.Type switch
		{
			LocationType.Chest => CheckChestLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.Cow => CheckCowLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.Skulltula => CheckSkulltulaLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.Shop => CheckShopLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.GroundItem => CheckGroundItemLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.Event => CheckEventLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.GetInfo => CheckGetInfoLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.InfoTable => CheckInfoTableLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.Scrubsanity => CheckScrubsanityLocation(
				locationInformation: locationInformation,
				memoryDictionary: memoryDictionary),
			LocationType.BossItem => CheckBossItemLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.BigPoeBottle => CheckBigPoeBottleLocation(
				memoryDictionary: memoryDictionary),
			LocationType.GreatFairy => CheckGreatFairyMagicLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.TrailGreatFairy => CheckTrailGreatFairyMagicLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.CraterGreatFairy => CheckCraterGreatFairyMagicLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.Medigoron => CheckMedigoronLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.BiggoronSword => CheckBiggoronSwordLocation(
				memoryDictionary: memoryDictionary),
			LocationType.BeanSale => CheckBeanSaleLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.FishingChild => CheckFishingLocation(
				isAdult: false,
				memoryDictionary: memoryDictionary),
			LocationType.FishingAdult => CheckFishingLocation(
				isAdult: true,
				memoryDictionary: memoryDictionary),
			LocationType.FireArrows => CheckFireArrowsLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			LocationType.MembershipCardCheck => CheckMembershipCardLocation(
				memoryDictionary: memoryDictionary),
			LocationType.BombchuSalesman => CheckBombchuSalesmanLocation(
				locationInformation: locationInformation,
				outgoingItemKey: outgoingItemKey,
				memoryDictionary: memoryDictionary),
			_ => throw new InvalidOperationException(
				$"Unknown LocationType {locationInformation.Type} for location {locationInformation.Name}"
			),
		};
	}

	private static bool CheckChestLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   ootrLocationType: 0x1)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0x0,
			   memoryDictionary: memoryDictionary);

	private static bool CheckGroundItemLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   ootrLocationType: 0x2)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	private static bool CheckBossItemLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x4F,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x1F,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	// updates save context immediately, so the existing client doesn't check temp context
	// need to figure out the temp context format eventually so that we can do temp context more efficiently later
	private static bool CheckScrubsanityLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
		=> SceneCheck(
			sceneOffset: locationInformation.Offset,
			bitToCheck: locationInformation.BitToCheck,
			sceneDataOffset: 0x10,
			memoryDictionary: memoryDictionary);

	private static bool CheckCowLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: (byte)(locationInformation.BitToCheck - 0x03),
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	private static bool CheckGreatFairyMagicLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0x04,
			   memoryDictionary: memoryDictionary);

	private static bool CheckTrailGreatFairyMagicLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: 0xFF,
			   bitToCheck: 0x13,
			   ootrLocationType: 0x05)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0x04,
			   memoryDictionary: memoryDictionary);

	private static bool CheckCraterGreatFairyMagicLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: 0xFF,
			   bitToCheck: 0x14,
			   ootrLocationType: 0x05)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0x04,
			   memoryDictionary: memoryDictionary);

	private static bool CheckMedigoronLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x16,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	private static bool CheckBeanSaleLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x16,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	private static bool CheckBombchuSalesmanLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x03,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0xC,
			   memoryDictionary: memoryDictionary);

	private static bool CheckSkulltulaLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
	{
		var skulltulaLocationAddress = GetSkulltulaLocationAddress(locationInformation);
		var nearbyMemory = memoryDictionary[skulltulaLocationAddress];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: locationInformation.BitToCheck);
	}

	// TODO: need to document and figure this out more
	private static long GetSkulltulaLocationAddress(LocationInformation locationInformation)
	{
		var skulltulaArrayIndex = locationInformation.Offset + 3 - 2 * (locationInformation.Offset % 4);
		var localSkulltulaOffset = AddressConstants.SkulltulaFlagsOffset + skulltulaArrayIndex;

		return localSkulltulaOffset;
	}

	private static bool CheckShopLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
	{
		var nearbyMemory = memoryDictionary[AddressConstants.ShopContextOffset];
		var bitToCheck = locationInformation.Offset * 4 + locationInformation.BitToCheck;

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: (byte)bitToCheck);
	}

	// Checked via looking at the points on the card, rather than getting the item itself
	private bool CheckBigPoeBottleLocation(Dictionary<long, long> memoryDictionary)
	{
		var bigPoesPoints = memoryDictionary[AddressConstants.BigPoePointsAddress];

		return bigPoesPoints >= _bigPoePointsRequired;
	}

	private static bool CheckEventLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
	{
		var eventAddress = AddressConstants.EventContextAddress + locationInformation.Offset * 2;
		var eventRow = memoryDictionary[eventAddress];

		return ByteUtils.CheckBit(
			memoryToCheck: eventRow,
			bitToCheck: locationInformation.BitToCheck);
	}

	private static bool CheckGetInfoLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
	{
		var itemGetInfoAddress = AddressConstants.GetInfoStartAddress + locationInformation.Offset;
		var nearbyMemory = memoryDictionary[itemGetInfoAddress];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: locationInformation.BitToCheck);
	}

	private static bool CheckInfoTableLocation(
		LocationInformation locationInformation,
		Dictionary<long, long> memoryDictionary
	)
	{
		var itemInfoTableAddress = AddressConstants.InfoTableStartAddress + locationInformation.Offset;
		var nearbyMemory = memoryDictionary[itemInfoTableAddress];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: locationInformation.BitToCheck);
	}

	private static bool CheckMembershipCardLocation(Dictionary<long, long> memoryDictionary)
	{
		const long eventAddress = AddressConstants.EventContextAddress + 0x9 * 2;

		var eventRow = memoryDictionary[eventAddress];

		return ByteUtils.CheckBit(
			       memoryToCheck: eventRow,
			       bitToCheck: 0)
		       && ByteUtils.CheckBit(
			       memoryToCheck: eventRow,
			       bitToCheck: 1)
		       && ByteUtils.CheckBit(
			       memoryToCheck: eventRow,
			       bitToCheck: 2)
		       && ByteUtils.CheckBit(
			       memoryToCheck: eventRow,
			       bitToCheck: 3);
	}

	private static bool CheckFishingLocation(bool isAdult, Dictionary<long, long> memoryDictionary)
	{
		var bitToCheck = isAdult ? 11 : 10;
		var nearbyMemory = memoryDictionary[AddressConstants.FishingContextAddress];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: (byte)bitToCheck);
	}

	private static bool CheckFireArrowsLocation(
		LocationInformation locationInformation,
		byte[] outgoingItemKey,
		Dictionary<long, long> memoryDictionary
	)
		=> OutgoingKeyCheck(
			   outgoingItemKey: outgoingItemKey,
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: 0x58,
			   ootrLocationType: 0x0)
		   || SceneCheck(
			   sceneOffset: locationInformation.Offset,
			   bitToCheck: locationInformation.BitToCheck,
			   sceneDataOffset: 0x0,
			   memoryDictionary: memoryDictionary);

	private static bool CheckBiggoronSwordLocation(Dictionary<long, long> memoryDictionary)
	{
		const byte bitToCheck = 8;

		var nearbyMemory = memoryDictionary[AddressConstants.EquipmentOffset];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: bitToCheck);
	}

	private static bool SceneCheck(
		byte sceneOffset,
		byte bitToCheck,
		byte sceneDataOffset,
		Dictionary<long, long> memoryDictionary
	)
	{
		var localSceneOffset = GetLocalSceneOffset(
			sceneOffset: sceneOffset,
			sceneDataOffset: sceneDataOffset);
		var nearbyMemory = memoryDictionary[localSceneOffset];

		return ByteUtils.CheckBit(
			memoryToCheck: nearbyMemory,
			bitToCheck: bitToCheck);
	}

	private static MemoryReadCommand GetLocalSceneMemoryReadCommand(byte sceneOffset, byte sceneDataOffset)
	{
		var localSceneOffset = GetLocalSceneOffset(
			sceneOffset: sceneOffset,
			sceneDataOffset: sceneDataOffset);

		return new MemoryReadCommand
		{
			Address = localSceneOffset,
			NumberOfBytes = 4,
		};
	}

	private static long GetLocalSceneOffset(byte sceneOffset, byte sceneDataOffset)
	{
		var localSceneOffset = AddressConstants.SceneFlagsOffset + 0x1c * sceneOffset + sceneDataOffset;

		return localSceneOffset;
	}

	private static bool OutgoingKeyCheck(
		byte[] outgoingItemKey,
		byte sceneOffset,
		byte bitToCheck,
		byte ootrLocationType
	) =>
		outgoingItemKey[0] == sceneOffset
		&& outgoingItemKey[1] == ootrLocationType
		&& outgoingItemKey[3] == bitToCheck;
}