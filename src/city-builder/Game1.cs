using System;
using city_builder.Rendering;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace city_builder;

public sealed class Game1 : Game
{
    private const int MapWidth = 10;
    private const int MapHeight = 10;
    private readonly GraphicsDeviceManager _graphics;
    private CityState _city = new(MapWidth, MapHeight);
    private SpriteBatch _spriteBatch = null!;
    private IsometricMapRenderer _mapRenderer = null!;
    private GameHud _hud = null!;
    private CellPosition? _hoveredCell;
    private MouseState _previousMouse;
    private bool _wasActive;
    private bool _actionHovered;
    private string? _message;
    private double _messageSeconds;

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
        Window.Title = "city builder";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var grassTexture = Content.Load<Texture2D>("Textures/Tiles/pasto");
        var buildingsTexture = Content.Load<Texture2D>("Textures/Buildings/pixel-city");
        _mapRenderer = new IsometricMapRenderer(GraphicsDevice, grassTexture, buildingsTexture);
        _hud = new GameHud(GraphicsDevice, Content.Load<SpriteFont>("Fonts/Ui"));
        _previousMouse = Mouse.GetState();
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsActive && (Keyboard.GetState().IsKeyDown(Keys.Escape)
            || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed))
        {
            Exit();
        }

        _messageSeconds = Math.Max(0, _messageSeconds - gameTime.ElapsedGameTime.TotalSeconds);
        if (_messageSeconds == 0)
        {
            _message = null;
        }

        var mapBounds = _hud.GetMapBounds(GraphicsDevice.Viewport.Bounds);
        _mapRenderer.UpdateLayout(_city.Map, mapBounds);

        var mouse = Mouse.GetState();
        var actionBounds = _city.IsFinished
            ? _hud.GetNewGameButtonBounds(GraphicsDevice.Viewport.Bounds)
            : _hud.GetNextTurnButtonBounds(GraphicsDevice.Viewport.Bounds);
        _actionHovered = IsActive && actionBounds.Contains(mouse.Position);
        var mousePosition = new System.Numerics.Vector2(mouse.X, mouse.Y);
        _hoveredCell = IsActive && !_city.IsFinished && mapBounds.Contains(mouse.X, mouse.Y)
            && _mapRenderer.TryGetCell(_city.Map, mousePosition, out var cell)
                ? cell
                : null;

        var leftClick = mouse.LeftButton == ButtonState.Pressed
            && _previousMouse.LeftButton == ButtonState.Released;
        if (IsActive && _wasActive && leftClick)
        {
            if (_city.IsFinished)
            {
                if (_actionHovered)
                {
                    StartNewGame();
                }
            }
            else if (_actionHovered)
            {
                _city.AdvanceTurn();
                _message = null;
                _messageSeconds = 0;
                if (_city.IsFinished)
                {
                    _actionHovered = false;
                }
            }
            else if (_hoveredCell is { } selectedCell)
            {
                var result = _city.TryConstruct(selectedCell, BuildingType.House);
                _message = result switch
                {
                    ConstructionResult.InsufficientFunds => "Dinero insuficiente",
                    ConstructionResult.Occupied => "Esta celda está ocupada",
                    _ => null
                };
                _messageSeconds = _message is null ? 0 : 2.5;
            }
        }

        _previousMouse = mouse;
        _wasActive = IsActive;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(24, 32, 42));
        _mapRenderer.UpdateLayout(_city.Map, _hud.GetMapBounds(GraphicsDevice.Viewport.Bounds));
        var canBuild = _hoveredCell is { } cell
            && _city.CheckConstruction(cell, BuildingType.House) == ConstructionResult.Success;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _mapRenderer.Draw(_spriteBatch, _city.Map, _hoveredCell, canBuild);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _hud.Draw(_spriteBatch, GraphicsDevice.Viewport.Bounds, _city, _message, _actionHovered);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void StartNewGame()
    {
        _city = new CityState(MapWidth, MapHeight);
        _hoveredCell = null;
        _actionHovered = false;
        _message = null;
        _messageSeconds = 0;
    }

    protected override void UnloadContent()
    {
        _hud?.Dispose();
        _mapRenderer?.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }
}
