using System;
using System.Numerics;
using city_builder.World;

namespace city_builder.Geometry;

public sealed class IsometricProjection
{
    public float TileWidth { get; }
    public float TileHeight { get; }
    public Vector2 Origin { get; }

    public IsometricProjection(float tileWidth, float tileHeight, Vector2 origin)
    {
        if (!float.IsFinite(tileWidth) || tileWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileWidth));
        }

        if (!float.IsFinite(tileHeight) || tileHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileHeight));
        }

        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Origin = origin;
    }

    public Vector2 CellToScreen(CellPosition cell) => Origin + new Vector2(
        (cell.X - cell.Y) * TileWidth / 2f,
        (cell.X + cell.Y) * TileHeight / 2f);

    public CellPosition ScreenToCell(Vector2 screenPosition)
    {
        var offset = screenPosition - Origin;
        var gridX = offset.Y / TileHeight + offset.X / TileWidth;
        var gridY = offset.Y / TileHeight - offset.X / TileWidth;

        // Floor keeps points just outside the top edges outside the map.
        return new CellPosition((int)MathF.Floor(gridX), (int)MathF.Floor(gridY));
    }
}
