using System;
using System.IO;
using city_builder.Persistence;
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
    private readonly SessionPersistence _persistence =
        SessionPersistence.FromFile(Path.Combine(AppContext.BaseDirectory, "database.json"));
    private CityState _city = new(MapWidth, MapHeight);
    private SpriteBatch _spriteBatch = null!;
    private IsometricMapRenderer _mapRenderer = null!;
    private GameHud _hud = null!;
    private CellPosition? _hoveredCell;
    private BuildingType _selectedBuilding = BuildingType.House;
    private MouseState _previousMouse;
    private bool _wasActive;
    private bool _actionHovered;
    private string? _message;
    private double _messageSeconds;
    private GameState _state = GameState.Menu;
    private bool _hasStarted;
    private KeyboardState _previousKeyboard;
    private ButtonState _previousBackButton;
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
        var parkTexture = Content.Load<Texture2D>("Textures/Buildings/park");
        var utilitiesTexture = Content.Load<Texture2D>("Textures/Buildings/municipal-buildings");
        var factoryTexture = Content.Load<Texture2D>("Textures/Buildings/factory");
        _mapRenderer = new IsometricMapRenderer(GraphicsDevice, grassTexture, buildingsTexture, parkTexture,
            utilitiesTexture, factoryTexture);
        _hud = new GameHud(GraphicsDevice, Content.Load<SpriteFont>("Fonts/Ui"));
        _previousMouse = Mouse.GetState();
        _previousKeyboard = Keyboard.GetState();
        _previousBackButton = GamePad.GetState(PlayerIndex.One).Buttons.Back;
        _menuBackground = Content.Load<Texture2D>("Textures/UI/menu_bg");
        _playButton = new Button(Content.Load<Texture2D>("Textures/UI/btn_jugar"));
        _exitButton = new Button(Content.Load<Texture2D>("Textures/UI/btn_salir"));
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();
        var backButton = GamePad.GetState(PlayerIndex.One).Buttons.Back;
        var backPressed = IsActive && (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape)
            || backButton == ButtonState.Pressed && _previousBackButton == ButtonState.Released);
        var leftClick = mouse.LeftButton == ButtonState.Pressed
            && _previousMouse.LeftButton == ButtonState.Released;

        switch (_state)
        {
            case GameState.Menu:
                UpdateMenu(mouse, leftClick, backPressed);
                break;
            case GameState.Playing:
                UpdatePlaying(gameTime, mouse, leftClick, backPressed);
                break;
        }

        _previousMouse = mouse;
        _previousKeyboard = keyboard;
        _previousBackButton = backButton;
        _wasActive = IsActive;
        base.Update(gameTime);
    }

    private void UpdateMenu(MouseState mouse, bool leftClick, bool backPressed)
    {
        if (backPressed)
        {
            Exit();
            return;
        }

        var bounds = GetMenuBounds();
        var scale = bounds.Width / (float)_menuBackground.Width;
        _playButton.SetLayout(new Point(bounds.Center.X, bounds.Center.Y + (int)(10 * scale)), scale);
        _exitButton.SetLayout(new Point(bounds.Center.X, bounds.Center.Y + (int)(80 * scale)), scale);
        _playButton.Update(mouse.Position, IsActive);
        _exitButton.Update(mouse.Position, IsActive);

        if (IsActive && _wasActive && leftClick)
        {
            if (_playButton.IsHovered)
            {
                if (!_hasStarted)
                {
                    StartNewGame();
                }
                _state = GameState.Playing;
            }
            else if (_exitButton.IsHovered)
            {
                Exit();
            }
        }
    }

    private void UpdatePlaying(GameTime gameTime, MouseState mouse, bool leftClick, bool backPressed)
    {
        if (backPressed)
        {
            _state = GameState.Menu;
            _hoveredCell = null;
            _actionHovered = false;
            return;
        }

        _messageSeconds = Math.Max(0, _messageSeconds - gameTime.ElapsedGameTime.TotalSeconds);
        if (_messageSeconds == 0)
        {
            _message = null;
        }

        var mapBounds = _hud.GetMapBounds(GraphicsDevice.Viewport.Bounds);
        _mapRenderer.UpdateLayout(_city.Map, mapBounds);

        var actionBounds = _city.IsFinished
            ? _hud.GetNewGameButtonBounds(GraphicsDevice.Viewport.Bounds)
            : _hud.GetNextTurnButtonBounds(GraphicsDevice.Viewport.Bounds);
        _actionHovered = IsActive && actionBounds.Contains(mouse.Position);
        var mousePosition = new System.Numerics.Vector2(mouse.X, mouse.Y);
        _hoveredCell = IsActive && !_city.IsFinished && mapBounds.Contains(mouse.X, mouse.Y)
            && _mapRenderer.TryGetCell(_city.Map, mousePosition, out var cell)
                ? cell
                : null;

        if (IsActive && _wasActive && leftClick)
        {
            if (_city.IsFinished)
            {
                if (_actionHovered)
                {
                    StartNewGame();
                }
            }
            else if (_hud.TryGetBuildingAt(GraphicsDevice.Viewport.Bounds, mouse.Position, out var building))
            {
                _selectedBuilding = building;
                _message = null;
                _messageSeconds = 0;
            }
            else if (_actionHovered)
            {
                var completedTurn = _city.Turn;
                if (_city.AdvanceTurn())
                {
                    _persistence.RecordResources(completedTurn, _city.Money, _city.Population,
                        _city.Electricity, _city.Water);
                }
                _message = null;
                _messageSeconds = 0;
                if (_city.IsFinished)
                {
                    _actionHovered = false;
                    _persistence.FinishSession(_city.Result!);
                }
            }
            else if (_hoveredCell is { } selectedCell)
            {
                var result = _city.TryConstruct(selectedCell, _selectedBuilding);
                if (result == ConstructionResult.Success)
                {
                    _persistence.RecordConstruction(selectedCell, _selectedBuilding);
                }

                _message = result switch
                {
                    ConstructionResult.InsufficientFunds => "Dinero insuficiente",
                    ConstructionResult.Occupied => "Esta celda está ocupada",
                    ConstructionResult.InsufficientElectricity => "Electricidad insuficiente",
                    ConstructionResult.InsufficientWater => "Agua insuficiente",
                    ConstructionResult.InsufficientPopulation =>
                        $"Se necesitan más de {BuildingCatalog.Get(_selectedBuilding).MinimumPopulation - 1} habitantes",
                    _ => null
                };
                _messageSeconds = _message is null ? 0 : 2.5;
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(24, 32, 42));

        if (_state == GameState.Menu)
        {
            DrawMenu();
        }
        else
        {
            DrawPlaying();
        }

        base.Draw(gameTime);
    }

    private Rectangle GetMenuBounds()
    {
        var viewport = GraphicsDevice.Viewport.Bounds;
        var scale = Math.Min(viewport.Width / (float)_menuBackground.Width,
            viewport.Height / (float)_menuBackground.Height);
        var width = (int)(_menuBackground.Width * scale);
        var height = (int)(_menuBackground.Height * scale);
        return new Rectangle(viewport.Center.X - width / 2, viewport.Center.Y - height / 2, width, height);
    }

    private void DrawMenu()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_menuBackground, GetMenuBounds(), Color.White);
        _playButton.Draw(_spriteBatch);
        _exitButton.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    private void DrawPlaying()
    {
        _mapRenderer.UpdateLayout(_city.Map, _hud.GetMapBounds(GraphicsDevice.Viewport.Bounds));
        var canBuild = _hoveredCell is { } cell
            && _city.CheckConstruction(cell, _selectedBuilding) == ConstructionResult.Success;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _mapRenderer.Draw(_spriteBatch, _city.Map, _hoveredCell, _selectedBuilding, canBuild);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _hud.Draw(_spriteBatch, GraphicsDevice.Viewport.Bounds, _city, _selectedBuilding, _message,
            _actionHovered, _persistence.Status);
        _spriteBatch.End();
    }

    private void StartNewGame()
    {
        _hasStarted = true;
        _city = new CityState(MapWidth, MapHeight);
        _hoveredCell = null;
        _selectedBuilding = BuildingType.House;
        _actionHovered = false;
        _message = null;
        _messageSeconds = 0;
        _persistence.StartNewSession();
    }

    public void WaitForPendingSaves() => _persistence.WaitForPendingSaves();

    protected override void UnloadContent()
    {
        _hud?.Dispose();
        _mapRenderer?.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }
}
