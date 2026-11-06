using System;

namespace city_builder.World;

public static class BuildingCatalog
{
    public static BuildingDefinition Get(BuildingType type) => type switch
    {
        BuildingType.House => new BuildingDefinition("Casa", 100, 4, 10, ElectricityChange: -1, WaterChange: -1),
        BuildingType.Shop => new BuildingDefinition("Tienda", 250, 0, 50, MinimumPopulation: 11,
            ElectricityChange: -2, WaterChange: -2),
        BuildingType.Park => new BuildingDefinition("Parque", 300, 0, -10, SatisfactionBenefit: 10),
        BuildingType.PowerPlant => new BuildingDefinition("Central eléctrica", 800, 0, 0, ElectricityChange: 20),
        BuildingType.WaterPlant => new BuildingDefinition("Planta de agua", 800, 0, 0, WaterChange: 20),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
