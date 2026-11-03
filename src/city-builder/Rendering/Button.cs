using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace city_builder.Rendering;

public sealed class Button
{
    private readonly Texture2D _texture;
    private Rectangle _bounds;

    public bool IsHovered { get; private set; }

    public Button(Texture2D texture)
    {
        _texture = texture;
        _bounds = new Rectangle(0, 0, texture.Width, texture.Height);
    }

    public void SetLayout(Point center, float scale)
    {
        var width = Math.Max(1, (int)(_texture.Width * scale));
        var height = Math.Max(1, (int)(_texture.Height * scale));
        _bounds = new Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
    }

    public void Update(Point mousePosition, bool isActive) => IsHovered = isActive && _bounds.Contains(mousePosition);

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_texture, _bounds, IsHovered ? Color.LightGray : Color.White);
}
