using Archipelago.RetroArchClient.OcarinaOfTime.Models;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Data;

public static class AllLocationInformation
{
    // This is now filled as we parse through the new Location Specific YAML files.
    public static List<LocationInformation> AllLocations { get; } = [];
}
