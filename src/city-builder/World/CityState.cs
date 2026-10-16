using System;

namespace city_builder.World;

public sealed class CityState
{
    public CityMap Map { get; }
    public int Money { get; private set; }
    public int Population { get; private set; }

    public CityState(int width, int height, int startingMoney = 1000)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startingMoney);
        Map = new CityMap(width, height);
        Money = startingMoney;
    }

    public ConstructionResult CheckConstruction(CellPosition position, BuildingType type)
    {
        var definition = BuildingCatalog.Get(type);

        if (!Map.Contains(position))
        {
            return ConstructionResult.OutsideMap;
        }

        if (!Map.CanPlaceBuilding(position))
        {
            return ConstructionResult.Occupied;
        }

        return Money >= definition.Cost
            ? ConstructionResult.Success
            : ConstructionResult.InsufficientFunds;
    }

    public ConstructionResult TryConstruct(CellPosition position, BuildingType type)
    {
        var result = CheckConstruction(position, type);
        if (result != ConstructionResult.Success)
        {
            return result;
        }

        if (!Map.TryPlaceBuilding(position, type))
        {
            return ConstructionResult.Occupied;
        }

        var definition = BuildingCatalog.Get(type);
        Money -= definition.Cost;
        Population += definition.PopulationBenefit;
        return ConstructionResult.Success;
    }
}
