using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.Configuration;

[JsonObject]
public record ArchipelagoServer(
        [property: JsonProperty("address")] string Address = "archipelago.gg",
        [property: JsonProperty("port")] int Port = 38281,
        [property: JsonProperty("slot_name")] string SlotName = "",
        [property: JsonProperty("password")] string Password = "");
