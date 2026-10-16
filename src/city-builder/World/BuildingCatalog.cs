using System;

namespace city_builder.World;

public static class BuildingCatalog
{
    public static BuildingDefinition Get(BuildingType type) => type switch
    {
        BuildingType.House => new BuildingDefinition(100, 4),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
