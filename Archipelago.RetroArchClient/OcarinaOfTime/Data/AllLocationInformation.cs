using System.Collections.Immutable;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Data;

public static class AllLocationInformation
{
    // This is now filled as we parse through the new Location Files.
    // Might put this as an immutable list at a point, for now, a normal list suffices.
    public static List<LocationInformation> AllLocations { get; } = [];
}
