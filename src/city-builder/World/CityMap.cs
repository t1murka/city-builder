using System;

namespace city_builder.World;

public sealed class CityMap
{
    private readonly MapCell[,] _cells;

    public int Width { get; }
    public int Height { get; }

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
}
