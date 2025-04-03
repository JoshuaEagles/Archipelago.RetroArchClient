using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;
using Archipelago.RetroArchClient.Services.Interfaces;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class ConfigurationService : IConfigurationService
{
	private readonly IUserPromptService _userPromptService;

    protected const string ExpectedConfigFileName = "config.json";

    public ConfigurationService(IUserPromptService userPromptService)
    {
		_userPromptService = userPromptService;
    }

    public ConfigurationSettings LoadConfigurationSettings()
    {
        var defaultSettings = new ConfigurationSettings();
        var currentDirectory = System.Environment.CurrentDirectory;
        if (string.IsNullOrWhiteSpace(currentDirectory))
        {
            Console.WriteLine("Could not determine current directory.");
            return defaultSettings;
        }

        var defaultFilePath = Path.Combine(currentDirectory, ExpectedConfigFileName);
		var filePath = _userPromptService.PromptForInput("path to config file", defaultFilePath);

        if (!FileUtils.TryLoadJsonFile<ConfigurationSettings>(filePath, out var configFile))
        {
            Console.WriteLine("Configuration file could not be loaded.");
            return defaultSettings;
        }

        return configFile;
    }

	public OoTClientConnectionSettings LoadOoTClientConnectionSettings(ConfigurationSettings configurationSettings)
	{

        var settings = new OoTClientConnectionSettings() 
        {
            ArchipelagoHostName = configurationSettings.ArchipelagoServer.Address,
            ArchipelagoPort = configurationSettings.ArchipelagoServer.Port,
            Password = configurationSettings.ArchipelagoServer.Password,
            SlotName = configurationSettings.ArchipelagoServer.SlotName,
            RetroArchHostName = configurationSettings.RetroArch.Address,
            RetroArchPort = configurationSettings.RetroArch.Port,
        };

        // TODO: Make this output pretty
        Console.WriteLine($"Current settings: {settings}");

        var shouldPromptManually = _userPromptService.PromptForBool("Would you like to update connection details manually?", false);

        if (!shouldPromptManually)
        {
            return settings;
        }

        return PromptForConnectionSettings(settings);
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

    private OoTClientConnectionSettings PromptForConnectionSettings(OoTClientConnectionSettings defaultSettings)
    {
        var defaultApHostname = defaultSettings.ArchipelagoHostName;
		var apHostName = _userPromptService.PromptForInput("Archipelago Server Hostname", defaultApHostname);

        var defaultApPort = defaultSettings.ArchipelagoPort;
        var apPortString = _userPromptService.PromptForInput("Archipelago Server port", defaultApPort.ToString());
        var apPort = string.IsNullOrWhiteSpace(apPortString) ? defaultApPort : int.Parse(apPortString);

        var defaultSlotName = defaultSettings.SlotName;
        var slotName = _userPromptService.PromptForInput("Slot Name", defaultSlotName);

        var defaultPassword = defaultSettings.Password;
        var password = _userPromptService.PromptForInput("Password", defaultPassword);

        var defaultRetroArchHostName = defaultSettings.RetroArchHostName;
        var retroArchHostname = _userPromptService.PromptForInput("RetroArch Hostname", defaultRetroArchHostName);

        var defaultRetroArchPort = defaultSettings.RetroArchPort;
        var retroArchPortString = _userPromptService.PromptForInput("RetroArch Port", defaultRetroArchPort.ToString());
        var retroArchPort = string.IsNullOrWhiteSpace(retroArchPortString) ? defaultRetroArchPort : int.Parse(retroArchPortString);

        return new OoTClientConnectionSettings
        {
            ArchipelagoHostName = apHostName,
            ArchipelagoPort = apPort,
            SlotName = slotName,
            Password = password,
            RetroArchHostName = retroArchHostname,
            RetroArchPort = retroArchPort,
        };
    }
}
