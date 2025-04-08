using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class ConfigurationService : IConfigurationService
{
	private readonly IUserPromptService _userPromptService;
	private readonly IFileService _fileService;

	// Could potentially make this configurable as well? 
    public const string DefaultConfigFileName = "config.json";

	public ConfigurationSettings DefaultConfigurationSettings 
	{
		get => new ConfigurationSettings(new ArchipelagoServer(), new RetroArch());
	}

    public ConfigurationService(IUserPromptService userPromptService, 
		IFileService fileService)
    {
        _userPromptService = userPromptService;
        _fileService = fileService;
    }

	/// <inheritdoc/>
    public ConfigurationSettings LoadConfigurationSettings()
    {
		try 
		{
			var defaultFilePath = _fileService.GetFilePathAtCurrentDirectory(DefaultConfigFileName);
			var filePath = _userPromptService.PromptForInput("path to config file", defaultFilePath);

			if (!_fileService.TryLoadJsonFile<ConfigurationSettings>(filePath, out var configFile))
			{
				Console.WriteLine("Configuration file could not be loaded.");
				return DefaultConfigurationSettings;
			}

			return configFile;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to load configuration file with exception: {ex}");
			return DefaultConfigurationSettings;
		}
    }
}
