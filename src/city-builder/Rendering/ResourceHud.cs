using System;
using city_builder.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace city_builder.Rendering;

public sealed class ResourceHud : IDisposable
{
    private const int Padding = 16;
    private const int CompactWidth = 440;
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

    public void Draw(SpriteBatch spriteBatch, Rectangle viewport, CityState city, string? message)
    {
        var panelHeight = GetPanelHeight(viewport.Width);
        var panel = new Rectangle(viewport.X, viewport.Y, viewport.Width, Math.Min(panelHeight, viewport.Height));
        spriteBatch.Draw(_pixel, panel, new Color(31, 42, 54));

        var compact = viewport.Width < CompactWidth;
        var columns = compact ? 2 : 3;
        var availableWidth = Math.Max(1, viewport.Width - Padding * 2);
        var columnWidth = availableWidth / (float)columns;
        var cost = BuildingCatalog.Get(BuildingType.House).Cost;
        var textColor = new Color(230, 236, 242);
        var costColor = city.Money >= cost ? new Color(176, 190, 203) : new Color(250, 142, 132);

        DrawLabel(spriteBatch, $"Dinero: ${city.Money}",
            new Vector2(viewport.X + Padding, viewport.Y + 15), columnWidth - 8, textColor);
        DrawLabel(spriteBatch, $"Población: {city.Population}",
            new Vector2(viewport.X + Padding + columnWidth, viewport.Y + 15), columnWidth - 8, textColor);
        DrawLabel(spriteBatch, $"Casa: ${cost}",
            new Vector2(viewport.X + Padding + (compact ? 0 : columnWidth * 2), viewport.Y + (compact ? 49 : 15)),
            compact ? availableWidth : columnWidth - 8, costColor);

        if (viewport.Height >= panelHeight + 40)
        {
            var hint = message ?? "Clic izquierdo: construir | Esc: salir";
            var hintColor = message is null ? new Color(157, 175, 191) : new Color(250, 142, 132);
            DrawLabel(spriteBatch, hint, new Vector2(viewport.X + Padding, viewport.Bottom - 28),
                availableWidth, hintColor, 0.85f);
        }
    }

    public void Dispose() => _pixel.Dispose();

    private static int GetPanelHeight(int width) => width < CompactWidth ? 88 : 56;

    private void DrawLabel(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth,
        Color color, float preferredScale = 1f)
    {
        var textWidth = _font.MeasureString(text).X;
        var scale = Math.Min(preferredScale, Math.Max(1f, maxWidth) / Math.Max(1f, textWidth));
        spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
