using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace city_builder
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _grassTexture;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _grassTexture = Content.Load<Texture2D>("Textures/Tiles/pasto");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            Vector2 tilePosition = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - _grassTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - _grassTexture.Height / 2f
            );

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                _grassTexture,
                tilePosition,
                Color.White
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}