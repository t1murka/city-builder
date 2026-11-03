using System;
using city_builder.Rendering;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NumericsVector2 = System.Numerics.Vector2;

namespace city_builder;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly CityState _city = new(10, 10);
    private SpriteBatch _spriteBatch = null!;
    private IsometricMapRenderer _mapRenderer = null!;
    private ResourceHud _hud = null!;
    private CellPosition? _hoveredCell;
    private MouseState _previousMouse;
    private bool _wasActive;
    private bool _nextTurnHovered;
    private string? _message;
    private double _messageSeconds;
    private GameState _state = GameState.Menu;
    private KeyboardState _previousKeyboard;
    private Texture2D _menuBackground = null!;
    private Button _playButton = null!;
    private Button _exitButton = null!;

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
        _hud = new ResourceHud(GraphicsDevice, Content.Load<SpriteFont>("Fonts/Ui"));
        _previousMouse = Mouse.GetState();
        _previousKeyboard = Keyboard.GetState();

        _menuBackground = Content.Load<Texture2D>("Textures/UI/menu_bg");
        _playButton = new Button(Content.Load<Texture2D>("Textures/UI/btn_jugar"));
        _exitButton = new Button(Content.Load<Texture2D>("Textures/UI/btn_salir"));
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();
        var escPressed = IsActive && keyboard.IsKeyDown(Keys.Escape)
            && _previousKeyboard.IsKeyUp(Keys.Escape);
        var leftClick = mouse.LeftButton == ButtonState.Pressed
            && _previousMouse.LeftButton == ButtonState.Released;

        switch (_state)
        {
            case GameState.Menu:
                UpdateMenu(mouse, leftClick, escPressed);
                break;
            case GameState.Playing:
                UpdatePlaying(gameTime, mouse, leftClick, escPressed);
                break;
            case GameState.Exiting:
                Exit();
                break;
        }

        _previousMouse = mouse;
        _previousKeyboard = keyboard;
        _wasActive = IsActive;

        base.Update(gameTime);
    }

    private void UpdateMenu(MouseState mouse, bool leftClick, bool escPressed)
    {
        if (escPressed)
        {
            _state = GameState.Exiting;
            return;
        }

        var viewport = GraphicsDevice.Viewport.Bounds;
        _playButton.SetCenter(new Point(viewport.Center.X, viewport.Height / 2 + 10));
        _exitButton.SetCenter(new Point(viewport.Center.X, viewport.Height / 2 + 80));

        _playButton.Update(mouse.Position);
        _exitButton.Update(mouse.Position);

        var clicked = IsActive && _wasActive && leftClick;
        if (clicked && _playButton.IsHovered)
        {
            _state = GameState.Playing;
        }
        else if (clicked && _exitButton.IsHovered)
        {
            _state = GameState.Exiting;
        }
    }

    private void UpdatePlaying(GameTime gameTime, MouseState mouse, bool leftClick, bool escPressed)
    {
        if (escPressed)
        {
            _state = GameState.Menu;
            return;
        }

        if (_messageSeconds > 0)
        {
            _messageSeconds -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_messageSeconds <= 0)
            {
                _message = null;
            }
        }

        var viewport = GraphicsDevice.Viewport.Bounds;
        var nextTurnBounds = _hud.GetNextTurnButtonBounds(viewport);
        _nextTurnHovered = nextTurnBounds.Contains(mouse.Position);

        var clicked = IsActive && _wasActive && leftClick;

        if (clicked && _nextTurnHovered)
        {
            _city.AdvanceTurn();
            return;
        }

        var mapBounds = _hud.GetMapBounds(viewport);
        var mouseInMap = mapBounds.Contains(mouse.Position);
        var screenPos = new NumericsVector2(mouse.X, mouse.Y);

        if (mouseInMap && _mapRenderer.TryGetCell(_city.Map, screenPos, out var cell))
        {
            _hoveredCell = cell;
            if (clicked)
            {
                var result = _city.TryConstruct(cell, BuildingType.House);
                if (result == ConstructionResult.InsufficientFunds)
                {
                    _message = "Fondos insuficientes para construir.";
                    _messageSeconds = 2.5;
                }
            }
        }
        else
        {
            _hoveredCell = null;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        switch (_state)
        {
            case GameState.Menu:
                DrawMenu();
                break;
            case GameState.Playing:
                DrawPlaying();
                break;
        }

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        GraphicsDevice.Clear(new Color(24, 32, 42));

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_menuBackground, GraphicsDevice.Viewport.Bounds, Color.White);
        _playButton.Draw(_spriteBatch);
        _exitButton.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    private void DrawPlaying()
    {
        GraphicsDevice.Clear(new Color(24, 32, 42));
        _mapRenderer.UpdateLayout(_city.Map, _hud.GetMapBounds(GraphicsDevice.Viewport.Bounds));
        var canBuild = _hoveredCell is { } cell
            && _city.CheckConstruction(cell, BuildingType.House) == ConstructionResult.Success;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _mapRenderer.Draw(_spriteBatch, _city.Map, _hoveredCell, canBuild);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _hud.Draw(_spriteBatch, GraphicsDevice.Viewport.Bounds, _city, _message, _nextTurnHovered);
        _spriteBatch.End();
    }

    protected override void UnloadContent()
    {
        _hud?.Dispose();
        _mapRenderer?.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }
}