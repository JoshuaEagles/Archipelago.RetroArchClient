using Archipelago.RetroArchClient.Configuration;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;

public interface IConfigurationService
{
    /// <summary>
    ///     Loads the <see cref="ConfigurationSettings" /> from the config file.
    ///     Currently, the file is expected to be located at "config.json"
    ///     wherever the program is running from.
    /// </summary>
    /// <returns>The loaded <see cref="ConfigurationSettings" /> object.</returns>
    ConfigurationSettings LoadConfigurationSettings();
}
