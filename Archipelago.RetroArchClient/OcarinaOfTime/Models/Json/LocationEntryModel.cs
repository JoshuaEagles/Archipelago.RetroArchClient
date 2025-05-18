using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models.Json;

/// <summary>
/// Represents a checkable entry within a location.
/// </summary>
public class LocationEntryModel
{
    /// <summary>
    /// The name of the location entry
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }
    
    [JsonProperty("offset")]
    private string? RawOffset { get; set; }
    
    [JsonProperty("bitToCheck")]
    private string? RawBitToCheck { get; set; }

    /// <summary>
    /// The offset of the location entry
    /// </summary>
    public byte Offset => Convert.ToByte(RawOffset ?? "0", 16);
    
    /// <summary>
    /// The bit we need to check.
    /// </summary>
    public byte BitToCheck => Convert.ToByte(RawBitToCheck ?? "0", 16);
}
