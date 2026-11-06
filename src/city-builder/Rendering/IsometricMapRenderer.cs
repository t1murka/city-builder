using System;
using city_builder.Geometry;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NumericsVector2 = System.Numerics.Vector2;

namespace city_builder.Rendering;

public sealed class IsometricMapRenderer : IDisposable
{
    private const int BaseTileWidth = 64;
    private const int BaseTileHeight = 32;
    private const int MapPadding = 48;

    private static readonly Rectangle HouseSource = new(80, 398, 34, 27);
    private static readonly Rectangle ShopSource = new(265, 374, 34, 23);
    private static readonly Rectangle PowerPlantSource = new(140, 136, 34, 31);
    private static readonly Rectangle WaterPlantSource = new(263, 138, 34, 32);
    private static readonly Rectangle FactorySource = new(15, 9, 35, 47);

    private readonly Texture2D _grassTexture;
    private readonly Texture2D _buildingsTexture;
    private readonly Texture2D _parkTexture;
    private readonly Texture2D _utilitiesTexture;
    private readonly Texture2D _factoryTexture;
    private readonly Texture2D _diamondTexture;
    private readonly Texture2D _pixelTexture;
    private IsometricProjection _projection = new(BaseTileWidth, BaseTileHeight, NumericsVector2.Zero);
    private Rectangle _viewport;
    private int _mapWidth;
    private int _mapHeight;

    public IsometricMapRenderer(GraphicsDevice graphicsDevice, Texture2D grassTexture,
        Texture2D buildingsTexture, Texture2D parkTexture, Texture2D utilitiesTexture, Texture2D factoryTexture)
    {
        _grassTexture = grassTexture;
        _buildingsTexture = buildingsTexture;
        _parkTexture = parkTexture;
        _utilitiesTexture = utilitiesTexture;
        _factoryTexture = factoryTexture;
        _diamondTexture = CreateDiamondTexture(graphicsDevice);
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public void UpdateLayout(CityMap map, Rectangle viewport)
    {
        if (_viewport == viewport && _mapWidth == map.Width && _mapHeight == map.Height)
        {
            return;
        }

        var mapWidth = (map.Width + map.Height) * BaseTileWidth / 2f;
        var mapHeight = (map.Width + map.Height) * BaseTileHeight / 2f;
        var availableWidth = Math.Max(1, viewport.Width - MapPadding * 2);
        var availableHeight = Math.Max(1, viewport.Height - MapPadding * 2);
        var scale = Math.Min(1f, Math.Min(availableWidth / mapWidth, availableHeight / mapHeight));
        var tileWidth = BaseTileWidth * scale;
        var tileHeight = BaseTileHeight * scale;
        var origin = new NumericsVector2(
            viewport.X + viewport.Width / 2f + (map.Height - map.Width) * tileWidth / 4f,
            viewport.Y + (viewport.Height - mapHeight * scale) / 2f);

        _projection = new IsometricProjection(tileWidth, tileHeight, origin);
        _viewport = viewport;
        _mapWidth = map.Width;
        _mapHeight = map.Height;
    }

    public bool TryGetCell(CityMap map, NumericsVector2 screenPosition, out CellPosition cell)
    {
        cell = _projection.ScreenToCell(screenPosition);
        return map.Contains(cell);
    }

    public void Draw(SpriteBatch spriteBatch, CityMap map, CellPosition? hoveredCell,
        BuildingType selectedBuilding, bool canBuild)
    {
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var cell = new CellPosition(x, y);
                var terrainTexture = map[cell].Terrain switch
                {
                    TerrainType.Grass => _grassTexture,
                    _ => throw new InvalidOperationException("Unsupported terrain type.")
                };

                DrawTile(spriteBatch, _diamondTexture, cell, new Color(55, 108, 48));
                DrawTile(spriteBatch, terrainTexture, cell, Color.White);
            }
        }

        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                DrawOutline(spriteBatch, new CellPosition(x, y), new Color(20, 50, 35) * 0.55f, 1f);
            }
        }

        if (hoveredCell is { } selectedCell && map.Contains(selectedCell))
        {
            var color = canBuild ? new Color(120, 245, 150) : new Color(255, 100, 100);
            DrawTile(spriteBatch, _diamondTexture, selectedCell, color * 0.22f);
            DrawOutline(spriteBatch, selectedCell, color, 2f);
        }

        DrawBuildings(spriteBatch, map, hoveredCell, selectedBuilding, canBuild);
    }

    public void Dispose()
    {
        // Loaded textures belong to ContentManager; only generated textures are disposed here.
        _diamondTexture.Dispose();
        _pixelTexture.Dispose();
    }

    private void DrawBuildings(SpriteBatch spriteBatch, CityMap map, CellPosition? hoveredCell,
        BuildingType selectedBuilding, bool canBuild)
    {
        // Diagonal traversal draws both buildings and the preview from back to front.
        for (var depth = 0; depth < map.Width + map.Height - 1; depth++)
        {
            var firstX = Math.Max(0, depth - map.Height + 1);
            var lastX = Math.Min(map.Width - 1, depth);

            for (var x = firstX; x <= lastX; x++)
            {
                var cell = new CellPosition(x, depth - x);
                if (map[cell].Building is { } building)
                {
                    DrawBuilding(spriteBatch, building, cell, Color.White);
                }

                if (hoveredCell == cell)
                {
                    var color = canBuild ? new Color(150, 255, 180) : new Color(255, 100, 100);
                    DrawBuilding(spriteBatch, selectedBuilding, cell, color * 0.65f);
                }
            }
        }
    }

    private void DrawBuilding(SpriteBatch spriteBatch, BuildingType building, CellPosition cell, Color color)
    {
        var source = building switch
        {
            BuildingType.House => HouseSource,
            BuildingType.Shop => ShopSource,
            BuildingType.Park => _parkTexture.Bounds,
            BuildingType.PowerPlant => PowerPlantSource,
            BuildingType.WaterPlant => WaterPlantSource,
            BuildingType.Factory => FactorySource,
            _ => throw new InvalidOperationException("Unsupported building type.")
        };

        var top = _projection.CellToScreen(cell);
        var bottom = new Vector2(top.X, top.Y + _projection.TileHeight);
        var origin = new Vector2(source.Width / 2f, source.Height);
        var scale = _projection.TileWidth / source.Width;

        var texture = building switch
        {
            BuildingType.Park => _parkTexture,
            BuildingType.PowerPlant or BuildingType.WaterPlant => _utilitiesTexture,
            BuildingType.Factory => _factoryTexture,
            _ => _buildingsTexture
        };
        spriteBatch.Draw(texture, bottom, source, color, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    private void DrawTile(SpriteBatch spriteBatch, Texture2D texture, CellPosition cell, Color color)
    {
        var top = _projection.CellToScreen(cell);
        var position = new Vector2(top.X - _projection.TileWidth / 2f, top.Y);
        var scale = new Vector2(_projection.TileWidth / texture.Width, _projection.TileHeight / texture.Height);

        spriteBatch.Draw(texture, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawOutline(SpriteBatch spriteBatch, CellPosition cell, Color color, float thickness)
    {
        var point = _projection.CellToScreen(cell);
        var top = new Vector2(point.X, point.Y);
        var right = top + new Vector2(_projection.TileWidth / 2f, _projection.TileHeight / 2f);
        var bottom = top + new Vector2(0f, _projection.TileHeight);
        var left = top + new Vector2(-_projection.TileWidth / 2f, _projection.TileHeight / 2f);

        DrawLine(spriteBatch, top, right, color, thickness);
        DrawLine(spriteBatch, right, bottom, color, thickness);
        DrawLine(spriteBatch, bottom, left, color, thickness);
        DrawLine(spriteBatch, left, top, color, thickness);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var direction = end - start;
        spriteBatch.Draw(_pixelTexture, start, null, color, MathF.Atan2(direction.Y, direction.X),
            new Vector2(0f, 0.5f), new Vector2(direction.Length(), thickness), SpriteEffects.None, 0f);
    }

    private static Texture2D CreateDiamondTexture(GraphicsDevice graphicsDevice)
    {
        var colors = new Color[BaseTileWidth * BaseTileHeight];

        for (var y = 0; y < BaseTileHeight; y++)
        {
            for (var x = 0; x < BaseTileWidth; x++)
            {
                var distance = MathF.Abs((x + 0.5f - BaseTileWidth / 2f) / (BaseTileWidth / 2f))
                    + MathF.Abs((y + 0.5f - BaseTileHeight / 2f) / (BaseTileHeight / 2f));
                colors[y * BaseTileWidth + x] = distance <= 1f ? Color.White : Color.Transparent;
            }
        }

        var texture = new Texture2D(graphicsDevice, BaseTileWidth, BaseTileHeight);
        texture.SetData(colors);
        return texture;
    }
}
