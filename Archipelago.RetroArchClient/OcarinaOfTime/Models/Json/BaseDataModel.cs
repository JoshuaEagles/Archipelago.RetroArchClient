using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models.Json;

/// <summary>
/// Contains a list for each entry within a location.
/// </summary>
public class BaseDataModel
{
    /// <summary>
    /// Gets or sets the list of entries associated with a specific location.
    /// Each entry represents an individual checkable area within that location.
    /// </summary>
    [JsonProperty("entries")]
    public List<LocationEntryModel>? Entries { get; set; }
}
