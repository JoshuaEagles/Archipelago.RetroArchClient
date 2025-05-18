using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Models.Json;

/// <summary>
/// Represents the type of a checkable location within an area.
/// </summary>
public class LocationTypeModel
{
    /// <summary>
    /// Represents all checkable chest locations within an area.
    /// </summary>
    [JsonProperty("chests")]
    public BaseDataModel? Chests { get; set; }
    
    /// <summary>
    /// Represents all checkable cow locations within an area.
    /// </summary>
    [JsonProperty("cows")]
    public BaseDataModel? Cows { get; set; }
    
    /// <summary>
    /// Represents all checkable golden skulltula locations within an area.
    /// </summary>
    [JsonProperty("skulltulas")]
    public BaseDataModel? Skulltulas { get; set; }
    
    /// <summary>
    /// Represents all checkable freestanding items within an area.
    /// </summary>
    [JsonProperty("groundItems")]
    public BaseDataModel? GroundItems { get; set; }
    
    // TODO: Describe what is meant with Event.
    [JsonProperty("events")]
    public BaseDataModel? Events { get; set; }
    
    // TODO: Describe what is meant with GetInfo
    [JsonProperty("getInfos")]
    public BaseDataModel? GetInfos { get; set; }
    
    // TODO: Describe what is meant with InfoTable.
    [JsonProperty("infoTables")]
    public BaseDataModel? InfoTables { get; set; }
    
    /// <summary>
    /// Represents all checkable deku scrub locations within an area.
    /// </summary>
    [JsonProperty("scrubsanity")]
    public BaseDataModel? Scrubsanities { get; set; }
    
    /// <summary>
    /// Represents the boss reward items after a boss has been defeated.
    /// </summary>
    [JsonProperty("bossItems")]
    public BaseDataModel? BossItems { get; set; }
    
    /// <summary>
    /// Represents the reward for the 10 Big Poes check in Adult Hyrule Market
    /// </summary>
    [JsonProperty("bigPoeBottle")]
    public BaseDataModel? BigPoeBottle { get; set; }
    
    /// <summary>
    /// Represents all checkable great fairy rewards within an area.
    /// </summary>
    [JsonProperty("greatFairies")]
    public BaseDataModel? GreatFairies { get; set; }
    
    /// <summary>
    /// Represents the great fairy reward on Death Mountain Trail.
    /// </summary>
    [JsonProperty("trailGreatFairy")]
    public BaseDataModel? TrailGreatFairy { get; set; }
    
    /// <summary>
    /// Represents the great fairy reward in Death Mountain Crater.
    /// </summary>
    [JsonProperty("craterGreatFairy")]
    public BaseDataModel? CraterGreatFairy { get; set; }
    
    /// <summary>
    /// Represents the item you get from Medigoron.
    /// </summary>
    [JsonProperty("medigoron")]
    public BaseDataModel? Medigoron { get; set; }
    
    /// <summary>
    /// Represents the reward you get from Biggoron after showing him the Claim Check
    /// </summary>
    [JsonProperty("biggoronSword")]
    public BaseDataModel? BiggoronSword { get; set; }
    
    /// <summary>
    /// Represents the item you get from the Bean Salesman at Zora's River
    /// </summary>
    [JsonProperty("beanSale")]
    public BaseDataModel? BeanSale { get; set; }
    
    /// <summary>
    /// Represents the reward you get from setting a new fishing record as child Link
    /// </summary>
    [JsonProperty("fishingChild")]
    public BaseDataModel? FishingChild { get; set; }
    
    /// <summary>
    /// Represents the reward you get from setting a new fishing record as adult Link
    /// </summary>
    [JsonProperty("fishingAdult")]
    public BaseDataModel? FishingAdult { get; set; }
    
    /// <summary>
    /// Represents the item you get after shooting an arrow into the sun at Lake Hylia
    /// </summary>
    [JsonProperty("fireArrows")]
    public BaseDataModel? FireArrows { get; set; }
    
    // TODO: Describe what is meant with MembershipCardCheck.
    [JsonProperty("membershipCardCheck")]
    public BaseDataModel? MembershipCardCheck { get; set; }
    
    /// <summary>
    /// Represents the item you get from the Bombchu Salesman at Haunted Wasteland
    /// </summary>
    [JsonProperty("bombchuSalesman")]
    public BaseDataModel? BombchuSalesman { get; set; }
}
