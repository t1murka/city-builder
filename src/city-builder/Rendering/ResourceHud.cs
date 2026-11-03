using System;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace city_builder.Rendering;

public sealed class ResourceHud : IDisposable
{
    private const int Padding = 16;
    private const int VerticalPadding = 12;
    private const int RowHeight = 36;
    private const int ControlHeight = 32;
    private const int NextTurnCellIndex = 5;
    private const int CompactWidth = 600;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public ResourceHud(GraphicsDevice graphicsDevice, SpriteFont font)
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

    public void Draw(SpriteBatch spriteBatch, Rectangle viewport, CityState city, string? message,
        bool nextTurnHovered)
    {
        var panelHeight = GetPanelHeight(viewport.Width);
        var panel = new Rectangle(viewport.X, viewport.Y, viewport.Width, Math.Min(panelHeight, viewport.Height));
        spriteBatch.Draw(_pixel, panel, new Color(31, 42, 54));

        var availableWidth = Math.Max(1, viewport.Width - Padding * 2);
        var cost = BuildingCatalog.Get(BuildingType.House).Cost;
        var textColor = new Color(230, 236, 242);
        var costColor = city.Money >= cost ? new Color(176, 190, 203) : new Color(250, 142, 132);
        var incomeColor = new Color(154, 218, 173);

        DrawCell(spriteBatch, viewport, 0, $"Dinero: ${city.Money}", textColor);
        DrawCell(spriteBatch, viewport, 1, $"Población: {city.Population}", textColor);
        DrawCell(spriteBatch, viewport, 2, $"Casa: ${cost}", costColor);
        DrawCell(spriteBatch, viewport, 3, $"Turno: {city.Turn}", textColor);
        DrawCell(spriteBatch, viewport, 4, $"Ingreso/turno: ${city.IncomePerTurn}", incomeColor);

        DrawNextTurnButton(spriteBatch, GetNextTurnButtonBounds(viewport), nextTurnHovered, textColor);

        if (viewport.Height >= panelHeight + 40)
        {
            var hint = message ?? "Clic izquierdo: construir | Esc: menú";
            var hintColor = message is null ? new Color(157, 175, 191) : new Color(250, 142, 132);
            DrawLabel(spriteBatch, hint, new Vector2(viewport.X + Padding, viewport.Bottom - 28),
                availableWidth, hintColor, 0.85f);
        }
    }

    public void Dispose() => _pixel.Dispose();

    private static int GetColumnCount(int width) => width < CompactWidth ? 2 : 3;

    private static float GetColumnWidth(int width) =>
        Math.Max(1, width - Padding * 2) / (float)GetColumnCount(width);

    private static int GetPanelHeight(int width) =>
        VerticalPadding * 2 + (NextTurnCellIndex / GetColumnCount(width) + 1) * RowHeight;

    private void DrawCell(SpriteBatch spriteBatch, Rectangle viewport, int index, string text, Color color)
    {
        var columns = GetColumnCount(viewport.Width);
        var columnWidth = GetColumnWidth(viewport.Width);
        var position = new Vector2(viewport.X + Padding + index % columns * columnWidth,
            viewport.Y + VerticalPadding + index / columns * RowHeight + (ControlHeight - _font.LineSpacing) / 2f);
        DrawLabel(spriteBatch, text, position, columnWidth - 8, color);
    }

    private void DrawNextTurnButton(SpriteBatch spriteBatch, Rectangle bounds, bool hovered, Color textColor)
    {
        if (bounds.IsEmpty)
        {
            return;
        }

        var buttonColor = hovered ? new Color(65, 96, 123) : new Color(48, 70, 90);
        spriteBatch.Draw(_pixel, bounds, buttonColor);
        const string text = "Siguiente turno";
        var textSize = _font.MeasureString(text);
        var scale = Math.Min(0.9f, Math.Max(1, bounds.Width - 16) / Math.Max(1f, textSize.X));
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
