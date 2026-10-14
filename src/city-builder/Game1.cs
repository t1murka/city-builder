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
    private MouseState _previousMouse;
    private bool _wasActive;

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
        var buildingsTexture = Content.Load<Texture2D>("Textures/Buildings/pixel-city");
        _mapRenderer = new IsometricMapRenderer(GraphicsDevice, grassTexture, buildingsTexture);
        _previousMouse = Mouse.GetState();
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

        var leftClick = mouse.LeftButton == ButtonState.Pressed
            && _previousMouse.LeftButton == ButtonState.Released;
        var buildingPlaced = IsActive && _wasActive && leftClick
            && hoveredCell is { } selectedCell
            && _map.TryPlaceBuilding(selectedCell, BuildingType.House);

        if (_hoveredCell != hoveredCell || buildingPlaced)
        {
            _hoveredCell = hoveredCell;
            UpdateWindowTitle();
        }

        _previousMouse = mouse;
        _wasActive = IsActive;

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
        var selection = "Mueve el cursor sobre el mapa";
        if (_hoveredCell is { } cell)
        {
            var availability = _map.CanPlaceBuilding(cell) ? "libre" : "ocupada";
            selection = $"Celda ({cell.X}, {cell.Y}): {availability}";
        }

        Window.Title = $"city builder | Casas: {_map.BuildingCount} | {selection} | Clic izq.: construir | Esc: salir";
    }
}
