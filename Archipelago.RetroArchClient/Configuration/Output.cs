using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.Configuration;

[JsonObject]
public record Output(
    [property: JsonProperty("verbose_output")] string VerboseOutput = "file_only");
