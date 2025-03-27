using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.Configuration;

[JsonObject]
public record RetroArch(
    [property: JsonProperty("address")] string Address  = "localhost",
    [property: JsonProperty("port")] int Port = 55355);
