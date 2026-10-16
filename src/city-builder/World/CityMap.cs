using System;

namespace city_builder.World;

public sealed class CityMap
{
    private readonly MapCell[,] _cells;

    public int Width { get; }
    public int Height { get; }
    public int BuildingCount { get; private set; }

    public CityMap(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
        _cells = new MapCell[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _cells[x, y] = new MapCell(TerrainType.Grass);
            }
        }
    }

    public MapCell this[CellPosition position] => _cells[position.X, position.Y];

    public bool Contains(CellPosition position) =>
        position.X >= 0 && position.X < Width
        && position.Y >= 0 && position.Y < Height;

    public bool CanPlaceBuilding(CellPosition position) =>
        Contains(position) && _cells[position.X, position.Y].Building is null;

    internal bool TryPlaceBuilding(CellPosition position, BuildingType building)
    {
        if (!Enum.IsDefined(building))
        {
            throw new ArgumentOutOfRangeException(nameof(building));
        }

        if (!CanPlaceBuilding(position))
        {
            return false;
        }

        _cells[position.X, position.Y] = _cells[position.X, position.Y] with { Building = building };
        BuildingCount++;
        return true;
    }
}
