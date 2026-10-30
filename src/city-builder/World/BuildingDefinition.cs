namespace city_builder.World;

public readonly record struct BuildingDefinition(
    string Name, int Cost, int PopulationBenefit, int IncomePerTurn,
    int MinimumPopulation = 0, int SatisfactionBenefit = 0);
