using Archipelago.RetroArchClient.OcarinaOfTime.Data;
using Archipelago.RetroArchClient.OcarinaOfTime.Enums;
using Archipelago.RetroArchClient.OcarinaOfTime.Models;
using Archipelago.RetroArchClient.OcarinaOfTime.Models.Json;
using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class JsonParserService
{
    public void ReadJsonFiles()
    {
        foreach (var file in Directory.GetFiles("./Locations"))
        {
            var sanitizedPath = file.Replace("\\", "/");
            var fileContent = File.ReadAllText(sanitizedPath);
            var data = JsonConvert.DeserializeObject<LocationDataModel>(fileContent);

            ReadJsonData(data);
        }
    }

    private void ReadJsonData(LocationDataModel? data)
    {
        if (data is null)
        {
            return;
        }

        var area = data.Area;
        var locationTypes = data.LocationTypes;

        if (locationTypes?.Chests is { Entries: not null })
        {
            foreach (var location in locationTypes.Chests.Entries.Select(chest =>
                         new LocationInformation(chest.Name!, LocationType.Chest, chest.Offset, chest.BitToCheck,
                             area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.Cows is { Entries: not null })
        {
            foreach (var location in locationTypes.Cows.Entries.Select(cow =>
                         new LocationInformation(cow.Name!, LocationType.Cow, cow.Offset, cow.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.Skulltulas is { Entries: not null })
        {
            foreach (var location in locationTypes.Skulltulas.Entries.Select(skulltula =>
                         new LocationInformation(skulltula.Name!, LocationType.Skulltula, skulltula.Offset,
                             skulltula.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.GroundItems is { Entries: not null })
        {
            foreach (var location in locationTypes.GroundItems.Entries.Select(groundItem =>
                         new LocationInformation(groundItem.Name!, LocationType.GroundItem, groundItem.Offset,
                             groundItem.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.Events is { Entries: not null })
        {
            foreach (var location in locationTypes.Events.Entries.Select(eventEntry =>
                         new LocationInformation(eventEntry.Name!, LocationType.Event, eventEntry.Offset,
                             eventEntry.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.GetInfos is { Entries: not null })
        {
            foreach (var location in locationTypes.GetInfos.Entries.Select(getInfo =>
                         new LocationInformation(getInfo.Name!, LocationType.GetInfo, getInfo.Offset,
                             getInfo.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.InfoTables is { Entries: not null })
        {
            foreach (var location in locationTypes.InfoTables.Entries.Select(infoTable =>
                         new LocationInformation(infoTable.Name!, LocationType.InfoTable, infoTable.Offset,
                             infoTable.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.Scrubsanities is { Entries: not null })
        {
            foreach (var location in locationTypes.Scrubsanities.Entries.Select(scrubsanity =>
                         new LocationInformation(scrubsanity.Name!, LocationType.Scrubsanity, scrubsanity.Offset,
                             scrubsanity.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.BossItems is { Entries: not null })
        {
            foreach (var location in locationTypes.BossItems.Entries.Select(bossItem =>
                         new LocationInformation(bossItem.Name!, LocationType.BossItem, bossItem.Offset,
                             bossItem.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.BigPoeBottle is { Entries: not null })
        {
            foreach (var location in locationTypes.BigPoeBottle.Entries.Select(bigPoeBottle =>
                         new LocationInformation(bigPoeBottle.Name!, LocationType.BigPoeBottle,
                             bigPoeBottle.Offset, bigPoeBottle.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.GreatFairies is { Entries: not null })
        {
            foreach (var location in locationTypes.GreatFairies.Entries.Select(greatFairy =>
                         new LocationInformation(greatFairy.Name!, LocationType.GreatFairy,
                             greatFairy.Offset, greatFairy.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.TrailGreatFairy is { Entries: not null })
        {
            foreach (var location in locationTypes.TrailGreatFairy.Entries.Select(trailGreatFairy =>
                         new LocationInformation(trailGreatFairy.Name!, LocationType.TrailGreatFairy,
                             trailGreatFairy.Offset, trailGreatFairy.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.CraterGreatFairy is { Entries: not null })
        {
            foreach (var location in locationTypes.CraterGreatFairy.Entries.Select(craterGreatFairy =>
                         new LocationInformation(craterGreatFairy.Name!, LocationType.CraterGreatFairy,
                             craterGreatFairy.Offset, craterGreatFairy.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.Medigoron is { Entries: not null })
        {
            foreach (var location in locationTypes.Medigoron.Entries.Select(medigoron =>
                         new LocationInformation(medigoron.Name!, LocationType.Medigoron,
                             medigoron.Offset, medigoron.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.BiggoronSword is { Entries: not null })
        {
            foreach (var location in locationTypes.BiggoronSword.Entries.Select(biggoronSword =>
                         new LocationInformation(biggoronSword.Name!, LocationType.BiggoronSword,
                             biggoronSword.Offset, biggoronSword.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.BeanSale is { Entries: not null })
        {
            foreach (var location in locationTypes.BeanSale.Entries.Select(beanSale =>
                         new LocationInformation(beanSale.Name!, LocationType.BeanSale,
                             beanSale.Offset, beanSale.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.FishingChild is { Entries: not null })
        {
            foreach (var location in locationTypes.FishingChild.Entries.Select(fishingChild =>
                         new LocationInformation(fishingChild.Name!, LocationType.FishingChild,
                             fishingChild.Offset, fishingChild.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.FishingAdult is { Entries: not null })
        {
            foreach (var location in locationTypes.FishingAdult.Entries.Select(fishingAdult =>
                         new LocationInformation(fishingAdult.Name!, LocationType.FishingAdult,
                             fishingAdult.Offset, fishingAdult.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.FireArrows is { Entries: not null })
        {
            foreach (var location in locationTypes.FireArrows.Entries.Select(fireArrows =>
                         new LocationInformation(fireArrows.Name!, LocationType.FireArrows,
                             fireArrows.Offset, fireArrows.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.MembershipCardCheck is { Entries: not null })
        {
            foreach (var location in locationTypes.MembershipCardCheck.Entries.Select(membershipCardCheck =>
                         new LocationInformation(membershipCardCheck.Name!, LocationType.MembershipCardCheck,
                             membershipCardCheck.Offset, membershipCardCheck.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }

        if (locationTypes?.BombchuSalesman is { Entries: not null })
        {
            foreach (var location in locationTypes.BombchuSalesman.Entries.Select(bombchuSalesman =>
                         new LocationInformation(bombchuSalesman.Name!, LocationType.BombchuSalesman,
                             bombchuSalesman.Offset, bombchuSalesman.BitToCheck, area)))
            {
                AllLocationInformation.AllLocations.Add(location);
            }
        }
    }
}
