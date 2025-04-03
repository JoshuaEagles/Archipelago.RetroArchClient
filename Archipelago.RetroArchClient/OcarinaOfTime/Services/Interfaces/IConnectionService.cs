using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;
public interface IConnectionService
{
	OoTClientConnectionSettings LoadOoTClientConnectionSettings(ConfigurationSettings configurationSettings);
}
