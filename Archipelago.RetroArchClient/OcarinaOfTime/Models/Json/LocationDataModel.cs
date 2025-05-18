using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models.Json;

/// <summary>
/// Represents a location that the player can visit.
/// </summary>
public class LocationDataModel
{
    [JsonProperty("area")]
    private string? RawArea { get; set; }
    
    /// <summary>
    /// Represents a type of checkable locations within an area
    /// (e.g. Golden Skulltulas, Chests, etc.)
    /// </summary>
    [JsonProperty("locations")]
    public LocationTypeModel? LocationTypes { get; set; }

    /// <summary>
    /// Represents the area where a location resides in.
    /// E.g. "KakarikoVillage" => Area.KakarikoVillage
    /// </summary>
    public Area Area => (Area)Enum.Parse(typeof(Area), RawArea ?? "Unknown");
}
