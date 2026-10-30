using System;

namespace city_builder.World;

public static class BuildingCatalog
{
    public static BuildingDefinition Get(BuildingType type) => type switch
    {
        BuildingType.House => new BuildingDefinition("Casa", 100, 4, 10),
        BuildingType.Shop => new BuildingDefinition("Tienda", 250, 0, 50, MinimumPopulation: 11),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
