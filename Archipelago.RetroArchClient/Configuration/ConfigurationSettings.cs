using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.Configuration;

public record ConfigurationSettings
{
    public ConfigurationSettings(ArchipelagoServer archipelagoServer, RetroArch retroArch, Output output)
    {
        ArchipelagoServer = archipelagoServer;
        RetroArch = retroArch;
        Output = output;
    }

    public ConfigurationSettings()
        : this(new ArchipelagoServer(), new RetroArch(), new Output()) { }

    [JsonProperty("archipelago_server")]
    public ArchipelagoServer ArchipelagoServer { get; private init; }

    [property: JsonProperty("retroarch")]
    public RetroArch RetroArch { get; private init; }

    [property: JsonProperty("output")]
    public Output Output { get; private init; }
}
