using System;
using city_builder.Persistence;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace city_builder.Rendering;

public sealed class GameHud : IDisposable
{
    private const int Padding = 16;
    private const int VerticalPadding = 12;
    private const int RowHeight = 36;
    private const int ControlHeight = 32;
    private const int NextTurnCellIndex = 5;
    private const int CompactWidth = 600;
    private const int ResultPanelWidth = 420;
    private const int ResultPanelHeight = 300;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private static readonly BuildingType[] BuildingOptions =
        { BuildingType.House, BuildingType.Shop, BuildingType.Park, BuildingType.PowerPlant, BuildingType.WaterPlant };

    public GameHud(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _font = font;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public Rectangle GetMapBounds(Rectangle viewport)
    {
        var height = Math.Min(GetPanelHeight(viewport.Width), viewport.Height);
        return new Rectangle(viewport.X, viewport.Y + height, viewport.Width, viewport.Height - height);
    }

    public Rectangle GetNextTurnButtonBounds(Rectangle viewport)
    {
        var columns = GetColumnCount(viewport.Width);
        var columnWidth = GetColumnWidth(viewport.Width);
        var width = Math.Max(1, Math.Min(180, (int)columnWidth - 8));
        var bounds = new Rectangle(viewport.Right - Padding - width,
            viewport.Y + VerticalPadding + NextTurnCellIndex / columns * RowHeight, width, ControlHeight);
        return viewport.Contains(bounds) ? bounds : Rectangle.Empty;
    }

    public Rectangle GetNewGameButtonBounds(Rectangle viewport)
    {
        var panel = GetResultPanelBounds(viewport);
        var scale = panel.Width / (float)ResultPanelWidth;
        var width = (int)(180 * scale);
        return new Rectangle(panel.Center.X - width / 2, panel.Y + (int)(236 * scale),
            width, (int)(36 * scale));
    }

    public Rectangle GetBuildingButtonBounds(Rectangle viewport, BuildingType building)
    {
        var index = Array.IndexOf(BuildingOptions, building);
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(building));
        }

        var columns = GetBuildingColumnCount(viewport.Width);
        var availableWidth = viewport.Width - Padding * 2 - 8 * (columns - 1);
        var width = Math.Max(1, Math.Min(160, availableWidth / columns));
        var bounds = new Rectangle(
            viewport.X + Padding + index % columns * (width + 8),
            viewport.Y + VerticalPadding + (GetBuildingRow(viewport.Width) + index / columns) * RowHeight,
            width, ControlHeight);
        return viewport.Contains(bounds) ? bounds : Rectangle.Empty;
    }

    public bool TryGetBuildingAt(Rectangle viewport, Point position, out BuildingType building)
    {
        foreach (var option in BuildingOptions)
        {
            if (GetBuildingButtonBounds(viewport, option).Contains(position))
            {
                building = option;
                return true;
            }
        }

        building = default;
        return false;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle viewport, CityState city, BuildingType selectedBuilding,
        string? message, bool actionHovered, DatabaseStatus databaseStatus)
    {
        var panelHeight = GetPanelHeight(viewport.Width);
        var panel = new Rectangle(viewport.X, viewport.Y, viewport.Width, Math.Min(panelHeight, viewport.Height));
        spriteBatch.Draw(_pixel, panel, new Color(31, 42, 54));

        var availableWidth = Math.Max(1, viewport.Width - Padding * 2);
        var definition = BuildingCatalog.Get(selectedBuilding);
        var textColor = new Color(230, 236, 242);
        var incomeColor = city.IncomePerTurn >= 0 ? new Color(154, 218, 173) : new Color(250, 142, 132);

        var moneyColor = city.Money < 0 ? new Color(250, 142, 132) : textColor;
        DrawCell(spriteBatch, viewport, 0, $"Dinero: {FormatMoney(city.Money)}", moneyColor);
        DrawCell(spriteBatch, viewport, 1, $"Hab.: {city.Population} | Satisf.: {city.Satisfaction}%", textColor);
        DrawCell(spriteBatch, viewport, 2, $"Turno: {city.Turn}/{CityState.MaxTurns}", textColor);
        DrawCell(spriteBatch, viewport, 3, $"Balance/turno: {FormatMoneyChange(city.IncomePerTurn)}", incomeColor);
        var utilityColor = city.Electricity == 0 || city.Water == 0
            ? new Color(250, 198, 132) : textColor;
        DrawCell(spriteBatch, viewport, 4, $"Luz: {city.Electricity} | Agua: {city.Water}", utilityColor);

        if (city.Result is { } result)
        {
            DrawResult(spriteBatch, viewport, result, actionHovered, textColor, databaseStatus);
            return;
        }

        foreach (var building in BuildingOptions)
        {
            DrawButton(spriteBatch, GetBuildingButtonBounds(viewport, building), BuildingCatalog.Get(building).Name,
                building == selectedBuilding, textColor);
        }

        var buttonText = city.Turn == CityState.MaxTurns ? "Finalizar partida" : "Siguiente turno";
        DrawButton(spriteBatch, GetNextTurnButtonBounds(viewport), buttonText, actionHovered, textColor);

        if (viewport.Height >= panelHeight + 40)
        {
            var details = $"{definition.Name}: ${definition.Cost}";
            if (definition.PopulationBenefit > 0)
            {
                details += $" | +{definition.PopulationBenefit} hab.";
            }
            details += $" | {FormatMoneyChange(definition.IncomePerTurn)}/turno";
            if (definition.SatisfactionBenefit > 0)
            {
                details += $" | +{definition.SatisfactionBenefit} satisfacción";
            }
            if (definition.MinimumPopulation > 0)
            {
                details += $" | Población > {definition.MinimumPopulation - 1}";
            }
            if (definition.ElectricityChange != 0)
            {
                details += $" | Luz: {definition.ElectricityChange:+0;-0}";
            }
            if (definition.WaterChange != 0)
            {
                details += $" | Agua: {definition.WaterChange:+0;-0}";
            }
            var hint = message ?? details;
            var hintColor = message is null ? new Color(157, 175, 191) : new Color(250, 142, 132);
            DrawLabel(spriteBatch, hint, new Vector2(viewport.X + Padding, viewport.Bottom - 28),
                availableWidth, hintColor, 0.85f);
        }

        if (viewport.Height >= panelHeight + 64)
        {
            DrawLabel(spriteBatch, GetDatabaseStatusText(databaseStatus),
                new Vector2(viewport.X + Padding, viewport.Bottom - 50), availableWidth,
                GetDatabaseStatusColor(databaseStatus), 0.75f);
        }
    }

    public void Dispose() => _pixel.Dispose();

    private static int GetColumnCount(int width) => width < CompactWidth ? 2 : 3;

    private static float GetColumnWidth(int width) =>
        Math.Max(1, width - Padding * 2) / (float)GetColumnCount(width);

    private static int GetBuildingRow(int width) => NextTurnCellIndex / GetColumnCount(width) + 1;

    private static int GetBuildingColumnCount(int width) => width < CompactWidth ? 3 : BuildingOptions.Length;

    private static int GetPanelHeight(int width)
    {
        var columns = GetBuildingColumnCount(width);
        var rows = (BuildingOptions.Length + columns - 1) / columns;
        return VerticalPadding * 2 + (GetBuildingRow(width) + rows) * RowHeight;
    }

    private static string FormatMoney(int amount) => amount >= 0 ? $"${amount}" : $"-${-(long)amount}";

    private static string FormatMoneyChange(int amount) =>
        amount >= 0 ? $"+{FormatMoney(amount)}" : FormatMoney(amount);

    private static Rectangle GetResultPanelBounds(Rectangle viewport)
    {
        var scale = Math.Min(1f, Math.Min(Math.Max(0, viewport.Width - Padding * 2) / (float)ResultPanelWidth,
            Math.Max(0, viewport.Height - Padding * 2) / (float)ResultPanelHeight));
        var width = (int)(ResultPanelWidth * scale);
        var height = (int)(ResultPanelHeight * scale);
        return new Rectangle(viewport.Center.X - width / 2, viewport.Center.Y - height / 2, width, height);
    }

    private void DrawResult(SpriteBatch spriteBatch, Rectangle viewport, CityResult result,
        bool buttonHovered, Color textColor, DatabaseStatus databaseStatus)
    {
        var panel = GetResultPanelBounds(viewport);
        var scale = panel.Width / (float)ResultPanelWidth;
        spriteBatch.Draw(_pixel, viewport, Color.Black * 0.7f);
        spriteBatch.Draw(_pixel, panel, new Color(31, 42, 54));

        DrawResultLabel(spriteBatch, panel, "Fin de partida", 22, textColor, 1.3f);
        DrawResultLabel(spriteBatch, panel, $"Población: {result.Population}", 76, textColor);
        DrawResultLabel(spriteBatch, panel, $"Dinero: {FormatMoney(result.Money)}", 112, textColor);
        DrawResultLabel(spriteBatch, panel, $"Satisfacción: {result.Satisfaction}%", 148, textColor);
        DrawResultLabel(spriteBatch, panel, $"Puntaje: {result.Score}", 184, new Color(154, 218, 173));
        DrawResultLabel(spriteBatch, panel, GetDatabaseStatusText(databaseStatus), 216,
            GetDatabaseStatusColor(databaseStatus), 0.7f);
        DrawButton(spriteBatch, GetNewGameButtonBounds(viewport), "Nueva partida", buttonHovered,
            textColor, 0.9f * scale);
    }

    private static string GetDatabaseStatusText(DatabaseStatus status) => status switch
    {
        DatabaseStatus.Disabled => "Guardado desactivado",
        DatabaseStatus.InvalidConfiguration => "Configuración de MySQL inválida",
        DatabaseStatus.Connecting => "Conectando a MySQL...",
        DatabaseStatus.Ready => "Partida registrada",
        DatabaseStatus.Unavailable => "Sin conexión a MySQL",
        DatabaseStatus.Saving => "Guardando partida...",
        DatabaseStatus.Saved => "Resultado guardado",
        DatabaseStatus.Failed => "No se pudo guardar la partida",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    private static Color GetDatabaseStatusColor(DatabaseStatus status) => status switch
    {
        DatabaseStatus.InvalidConfiguration or DatabaseStatus.Unavailable or DatabaseStatus.Failed =>
            new Color(250, 142, 132),
        DatabaseStatus.Saved => new Color(154, 218, 173),
        _ => new Color(157, 175, 191)
    };

    private void DrawResultLabel(SpriteBatch spriteBatch, Rectangle panel, string text, float y,
        Color color, float preferredScale = 1f)
    {
        var panelScale = panel.Width / (float)ResultPanelWidth;
        var textWidth = _font.MeasureString(text).X;
        var scale = Math.Min(preferredScale * panelScale,
            Math.Max(1f, panel.Width - Padding * 2 * panelScale) / Math.Max(1f, textWidth));
        var position = new Vector2(panel.Center.X - textWidth * scale / 2, panel.Y + y * panelScale);
        spriteBatch.DrawString(_font, text, position, color,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawCell(SpriteBatch spriteBatch, Rectangle viewport, int index, string text, Color color)
    {
        var columns = GetColumnCount(viewport.Width);
        var columnWidth = GetColumnWidth(viewport.Width);
        var position = new Vector2(viewport.X + Padding + index % columns * columnWidth,
            viewport.Y + VerticalPadding + index / columns * RowHeight + (ControlHeight - _font.LineSpacing) / 2f);
        DrawLabel(spriteBatch, text, position, columnWidth - 8, color);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string text, bool hovered,
        Color textColor, float preferredScale = 0.9f)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var buttonColor = hovered ? new Color(65, 96, 123) : new Color(48, 70, 90);
        spriteBatch.Draw(_pixel, bounds, buttonColor);
        var textSize = _font.MeasureString(text);
        var scale = Math.Min(preferredScale, Math.Max(1, bounds.Width - 16) / Math.Max(1f, textSize.X));
        var position = new Vector2(bounds.Center.X, bounds.Center.Y) - textSize * scale / 2;
        spriteBatch.DrawString(_font, text, position, textColor,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawLabel(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth,
        Color color, float preferredScale = 1f)
    {
        var textWidth = _font.MeasureString(text).X;
        var scale = Math.Min(preferredScale, Math.Max(1f, maxWidth) / Math.Max(1f, textWidth));
        spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
