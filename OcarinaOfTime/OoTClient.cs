using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json.Linq;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Services;
using Archipelago.RetroArchClient.Services;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime;

public class OoTClient
{
	private readonly ArchipelagoSession _apSession;
	private readonly DeathLinkService _archipelagoDeathLinkService;
	private readonly CollectibleCheckService _collectibleCheckService;

	private readonly OoTClientConnectionSettings _connectionSettings;
	private readonly GameCompleteService _gameCompleteService;
	private readonly GameModeService _gameModeService;
	private readonly LocationCheckService _locationCheckService;
	private readonly IMemoryService _memoryService;
	private readonly OoTClientDeathLinkService _ootClientDeathLinkService;
	private readonly PlayerNameService _playerNameService;
	private readonly ReceiveItemService _receiveItemService;

	public OoTClient()
	{
		_connectionSettings = PromptForConnectionSettings();

		var udpClient = new UdpClient();
		udpClient.Connect(hostname: _connectionSettings.RetroArchHostName, port: _connectionSettings.RetroArchPort);

		_memoryService = new RetroArchMemoryService(
			udpClient: udpClient);
		_playerNameService = new PlayerNameService(
			memoryService: _memoryService);
		var currentSceneService = new CurrentSceneService(
			memoryService: _memoryService);
		_receiveItemService = new ReceiveItemService(
			memoryService: _memoryService,
			currentSceneService: currentSceneService
		);
		_gameModeService = new GameModeService(
			memoryService: _memoryService);
		_locationCheckService = new LocationCheckService(
			memoryService: _memoryService,
			gameModeService: _gameModeService
		);
		_collectibleCheckService = new CollectibleCheckService(
			memoryService: _memoryService);
		_ootClientDeathLinkService = new OoTClientDeathLinkService(
			memoryService: _memoryService,
			gameModeService: _gameModeService,
			currentSceneService: currentSceneService
		);
		_gameCompleteService = new GameCompleteService(
			memoryService: _memoryService);

		_apSession = ArchipelagoSessionFactory.CreateSession(
			hostname: _connectionSettings.ArchipelagoHostName,
			port: _connectionSettings.ArchipelagoPort
		);
		var loginResult = _apSession.TryConnectAndLogin(
			game: "Ocarina of Time",
			name: _connectionSettings.SlotName,
			itemsHandlingFlags: ItemsHandlingFlags.RemoteItems
		);
		_archipelagoDeathLinkService = _apSession.CreateDeathLinkService();

		if (!loginResult.Successful)
		{
			var loginFailure = (LoginFailure)loginResult;
			throw new Exception(
				$"Connection to Archipelago failed. Error message(s):{Environment.NewLine}{string.Join(separator: Environment.NewLine, value: loginFailure.Errors)}{Environment.NewLine}"
			);
		}

		Console.WriteLine("Connected to Archipelago");
	}

	[DoesNotReturn]
	public async Task RunClient()
	{
		var slotData = await GetSlotData();

		await WritePlayerNames(
			apSession: _apSession, 
			playerNameService: _playerNameService);

		// Setup DeathLink
		await _ootClientDeathLinkService.StoreDeathLinkEnabledFromMemory();
		var deathLinkEnabled = _ootClientDeathLinkService.DeathLinkEnabled;
		
		Console.WriteLine($"DeathLink {(deathLinkEnabled ? "is" : "is not")} enabled.");
		
		if (deathLinkEnabled)
		{
			_archipelagoDeathLinkService.EnableDeathLink();
			_archipelagoDeathLinkService.OnDeathLinkReceived += _ =>
			{
				_ootClientDeathLinkService.ReceiveDeathLink();
				Console.WriteLine("Death link received");
			};
		}

		await _locationCheckService.InitializeMasterQuestHandling();
		await _locationCheckService.InitializeBigPoesRequired();

		var clientSideReceivedItemsCount = -1;
		var isGameCompletionSent = false;
		var wasPreviouslyInGame = false;

		while (true)
		{
			await Task.Delay(50);

			// Handle detecting resets and reinitialization
			var currentGameMode = await _gameModeService.GetCurrentGameMode();
			
			if (!currentGameMode.IsInGame)
			{
				wasPreviouslyInGame = false;
				continue;
			}

			if (!wasPreviouslyInGame)
			{
				wasPreviouslyInGame = true;
				await WritePlayerNames(apSession: _apSession, playerNameService: _playerNameService);
				clientSideReceivedItemsCount = -1;
			}

			// Handle completing location checks
			var checkedLocationIds = await GetAllCheckedLocationIds(slotData);
			await _apSession.Locations.CompleteLocationChecksAsync(checkedLocationIds);

			// Receive Items
			var gameReceivedItemsCount = await _receiveItemService.GetLocalReceivedItemIndex();
			
			if (gameReceivedItemsCount > clientSideReceivedItemsCount)
			{
				currentGameMode = await _gameModeService.GetCurrentGameMode();
				
				if (!currentGameMode.IsInGame)
				{
					continue;
				}

				var canReceiveItem = await _receiveItemService.CanReceiveItem();
				
				if (canReceiveItem && _apSession.Items.Index > gameReceivedItemsCount)
				{
					clientSideReceivedItemsCount = gameReceivedItemsCount;

					var itemToReceive = _apSession.Items.AllItemsReceived[gameReceivedItemsCount];
					await _receiveItemService.ReceiveItem(itemToReceive);
				}
			}

			// Handle DeathLink
			var shouldSendDeathLink = await _ootClientDeathLinkService.ProcessDeathLink();
			
			if (shouldSendDeathLink)
			{
				var deathLink = new DeathLink(_connectionSettings.SlotName);
				_archipelagoDeathLinkService.SendDeathLink(deathLink);
				Console.WriteLine("Death link sent.");
			}

			// Handle Game Completion
			if (isGameCompletionSent)
			{
				continue;
			}
			
			var isGameComplete = await _gameCompleteService.IsGameComplete();

			currentGameMode = await _gameModeService.GetCurrentGameMode();
			
			if (!currentGameMode.IsInGame)
			{
				continue;
			}

			if (!isGameComplete)
			{
				continue;
			}
			
			_apSession.SetGoalAchieved();
			isGameCompletionSent = true;
			Console.WriteLine("Game completed");
		}
	}

	// performance improvement idea:
	// only check save context for locations on area changes, otherwise only use the temp context checks
	// should do this skip inside the function for each check type, so that checks that don't have temp context still get checked for
	// with how fast it is now, this would only be worth it for battery usage reasons

	// idea for receiving local items:
	// could have a sort of local database of checked locations, might want that anyway for performance reasons
	// any location in the local save file that is checked would be in there, but if you make a new save then there
	// could be locations checked in the multiworld that aren't marked as checked in the local database
	// the idea would be that when processing the item queue, we can check against the local database,
	// if the location is marked as checked there then that means we don't give the item, if it's not marked as checked then we do give the item
	// this would avoid giving duplicate items but mean we can receive local items when making a new save file

	private static OoTClientConnectionSettings PromptForConnectionSettings()
	{
		Console.WriteLine("Enter the Archipelago Server Hostname, default: archipelago.gg");
		var apHostname = Console.ReadLine();
		
		if (string.IsNullOrWhiteSpace(apHostname))
		{
			apHostname = "archipelago.gg";
		}

		// 38281 is the port where Archipelago runs if locally hosted.
		// This is useful for testing the client on a local server.
		// However, on the long run, it might be better to have this be configurable
		// through a config file and make it so the client can select the port
		// on its own through this.
		Console.WriteLine("Enter the Archipelago Server port, default: 38281");
		var apPortString = Console.ReadLine();
		var apPort = string.IsNullOrWhiteSpace(apPortString) ? 38281 : int.Parse(apPortString);

		Console.WriteLine("Enter the Slot Name, default: Player");
		var slotName = Console.ReadLine();
		
		if (string.IsNullOrEmpty(slotName))
		{
			slotName = "Player";
		}

		Console.WriteLine("Enter the RetroArch Hostname, default: localhost");
		var retroArchHostname = Console.ReadLine();
		
		if (string.IsNullOrEmpty(retroArchHostname))
		{
			retroArchHostname = "localhost";
		}

		Console.WriteLine("Enter the RetroArch Port, default: 55355");
		var retroArchPortString = Console.ReadLine();
		var retroArchPort = string.IsNullOrWhiteSpace(retroArchPortString) ? 55355 : int.Parse(retroArchPortString);

		return new OoTClientConnectionSettings
		{
			ArchipelagoHostName = apHostname,
			ArchipelagoPort = apPort,
			SlotName = slotName,
			RetroArchHostName = retroArchHostname,
			RetroArchPort = retroArchPort,
		};
	}

	private async Task<OoTClientSlotData> GetSlotData()
	{
		var slotData = await _apSession.DataStorage.GetSlotDataAsync();

		// Defaults to false if not found to support OOT but it's just master quest water temple
		var scrubsanityEnabled = slotData.ContainsKey("shuffle_scrubs") && (long)slotData["shuffle_scrubs"] >= 1;

		// Reading from the 0x8000000 address range would be valid, it's the same memory as 0xA0000000, just keeping all accesses in 0xA0000000 for consistency
		var collectibleOverridesFlagsAddress
			= await _memoryService.Read32(
				address: 0xA0400000 + Convert.ToUInt32(slotData["collectible_override_flags"])
			) - 0x80000000 + 0xA0000000;

		var collectibleFlagOffsets
			= SlotDataCollectableFlagOffsetsToArray(
				slotDataCollectibleFlagOffsets: slotData["collectible_flag_offsets"] as JObject);

		return new OoTClientSlotData
		{
			ShuffleScrubs = scrubsanityEnabled,
			CollectibleOverridesFlagsAddress = collectibleOverridesFlagsAddress,
			CollectibleFlagOffsets = collectibleFlagOffsets,
		};

		static List<CollectibleFlagOffset> SlotDataCollectableFlagOffsetsToArray(
			JObject? slotDataCollectibleFlagOffsets
		)
		{
			if (slotDataCollectibleFlagOffsets is null)
			{
				Console.WriteLine("SlotDataCollectibleFlagOffsets was null, this shouldn't happen!");
				return [];
			}

			var convertedCollectibleFlagOffsets = new List<CollectibleFlagOffset>(slotDataCollectibleFlagOffsets.Count);

			foreach (var flagOffsetData in slotDataCollectibleFlagOffsets)
			{
				// The slot data seems to always have a null key in it unless all collectible checks are enabled
				if (flagOffsetData.Key == "null")
				{
					continue;
				}

				var itemId = long.Parse(flagOffsetData.Key);

				if (flagOffsetData.Value is not JArray jArray)
				{
					Console.WriteLine("Null JArray in collectible flag offsets, this shouldn't happen!");
					continue;
				}

				var offset = jArray[0].Value<long>();
				var flag = jArray[1].Value<long>();

				convertedCollectibleFlagOffsets.Add(
					new CollectibleFlagOffset
					{
						ItemId = itemId,
						Offset = offset,
						Flag = flag,
					}
				);
			}

			return convertedCollectibleFlagOffsets;
		}
	}

	private static async Task WritePlayerNames(ArchipelagoSession apSession, PlayerNameService playerNameService)
	{
		var playerNames = apSession.Players.AllPlayers
			.Skip(1)
			.Select(x => x.Name);
		
		var nameIndex = 1; // the names are 1 indexed, nothing is stored at index 0
		
		foreach (var name in playerNames)
		{
			if (nameIndex >= 255)
			{
				break;
			}

			await playerNameService.WritePlayerName(
				index: (byte)nameIndex, 
				name: name);
			nameIndex++;
		}

		await playerNameService.WritePlayerName(
			index: 255, 
			name: "APPlayer");
		Console.WriteLine("Player names written");
	}

	private async Task<long[]> GetAllCheckedLocationIds(OoTClientSlotData slotData)
	{
		var checkedLocationNames = await _locationCheckService.GetAllCheckedLocationNames(slotData);

		var checkedLocationIds = checkedLocationNames
			.Select(
				locationName => _apSession.Locations.GetLocationIdFromName(
					game: "Ocarina of Time",
					locationName: locationName
				)
			);

		var checkedCollectibleIds = (await _collectibleCheckService.GetAllCheckedCollectibleIds(
			collectibleOverridesFlagAddress: slotData.CollectibleOverridesFlagsAddress,
			collectibleFlagOffsets: slotData.CollectibleFlagOffsets
		)).Select(id => id);

		return checkedLocationIds.Concat(checkedCollectibleIds)
			.ToArray();
	}
}
