namespace city_builder.World;

public sealed record CityResult(int Population, int Money)
{
    public const int PointsPerResident = 100;

    public long Score => (long)Population * PointsPerResident + Money;
}
