using Archipelago.RetroArchClient.Configuration;
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

}
