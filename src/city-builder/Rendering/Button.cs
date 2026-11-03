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

    public void SetCenter(Point center) =>
        _bounds.Location = new Point(center.X - _bounds.Width / 2, center.Y - _bounds.Height / 2);

    public void Update(Point mousePosition) => IsHovered = _bounds.Contains(mousePosition);

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_texture, _bounds, IsHovered ? Color.LightGray : Color.White);

}



