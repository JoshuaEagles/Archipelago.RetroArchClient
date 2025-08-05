using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.Configuration;

public record ConfigurationSettings
{
    public ConfigurationSettings(ArchipelagoServer archipelagoServer, RetroArch retroArch)
    {
        ArchipelagoServer = archipelagoServer;
        RetroArch = retroArch;
    }

    public ConfigurationSettings()
        : this(new ArchipelagoServer(), new RetroArch())
    {
    }

    [JsonProperty("archipelago_server")] public ArchipelagoServer ArchipelagoServer { get; private init; }

    [JsonProperty("retroarch")] public RetroArch RetroArch { get; private init; }
}
