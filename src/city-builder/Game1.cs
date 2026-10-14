using city_builder.Rendering;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace city_builder;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly CityMap _map = new(10, 10);
    private SpriteBatch _spriteBatch = null!;
    private IsometricMapRenderer _mapRenderer = null!;
    private CellPosition? _hoveredCell;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 960,
            PreferredBackBufferHeight = 640
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        UpdateWindowTitle();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var grassTexture = Content.Load<Texture2D>("Textures/Tiles/pasto");
        _mapRenderer = new IsometricMapRenderer(GraphicsDevice, grassTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsActive && (Keyboard.GetState().IsKeyDown(Keys.Escape)
            || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed))
        {
            Exit();
        }

        _mapRenderer.UpdateLayout(_map, GraphicsDevice.Viewport.Bounds);

        var mouse = Mouse.GetState();
        var mousePosition = new System.Numerics.Vector2(mouse.X, mouse.Y);
        CellPosition? hoveredCell = IsActive
            && GraphicsDevice.Viewport.Bounds.Contains(mouse.X, mouse.Y)
            && _mapRenderer.TryGetCell(_map, mousePosition, out var cell)
                ? cell
                : null;

        if (_hoveredCell != hoveredCell)
        {
            _hoveredCell = hoveredCell;
            UpdateWindowTitle();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(24, 32, 42));
        _mapRenderer.UpdateLayout(_map, GraphicsDevice.Viewport.Bounds);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _mapRenderer.Draw(_spriteBatch, _map, _hoveredCell);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _mapRenderer?.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private void UpdateWindowTitle()
    {
        var selection = _hoveredCell is { } cell
            ? $"Celda ({cell.X}, {cell.Y})"
            : "Mueve el cursor sobre el mapa";

        Window.Title = $"Constructor de ciudades | Mapa {_map.Width}x{_map.Height} | {selection} | Esc: salir";
    }
}
