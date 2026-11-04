using System;

namespace city_builder.World;

public sealed class CityState
{
    public const int MaxTurns = 20;

    public CityMap Map { get; }
    public int Money { get; private set; }
    public int Population { get; private set; }
    public int Turn { get; private set; } = 1;
    public int IncomePerTurn { get; private set; }
    public int Satisfaction { get; private set; } = 50;
    public int Electricity { get; private set; } = 10;
    public int Water { get; private set; } = 10;
    public CityResult? Result { get; private set; }
    public bool IsFinished => Result is not null;

    public CityState(int width, int height, int startingMoney = 1000)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startingMoney);
        Map = new CityMap(width, height);
        Money = startingMoney;
    }

    public ConstructionResult CheckConstruction(CellPosition position, BuildingType type)
    {
        var definition = BuildingCatalog.Get(type);

        if (IsFinished)
        {
            return ConstructionResult.SessionFinished;
        }

        if (!Map.Contains(position))
        {
            return ConstructionResult.OutsideMap;
        }

        if (!Map.CanPlaceBuilding(position))
        {
            return ConstructionResult.Occupied;
        }

        if (Population < definition.MinimumPopulation)
        {
            return ConstructionResult.InsufficientPopulation;
        }

        if (Money < definition.Cost)
        {
            return ConstructionResult.InsufficientFunds;
        }

        if (Electricity + definition.ElectricityChange < 0)
        {
            return ConstructionResult.InsufficientElectricity;
        }

        return Water + definition.WaterChange < 0
            ? ConstructionResult.InsufficientWater
            : ConstructionResult.Success;
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
        IncomePerTurn += definition.IncomePerTurn;
        Satisfaction = Math.Clamp(Satisfaction + definition.SatisfactionBenefit, 0, 100);
        Electricity += definition.ElectricityChange;
        Water += definition.WaterChange;
        return ConstructionResult.Success;
    }

    public bool AdvanceTurn()
    {
        if (IsFinished)
        {
            return false;
        }

        Money = checked(Money + IncomePerTurn);
        if (Turn == MaxTurns)
        {
            Result = new CityResult(Population, Money, Satisfaction);
        }
        else
        {
            Turn++;
        }

        return true;
    }
}
