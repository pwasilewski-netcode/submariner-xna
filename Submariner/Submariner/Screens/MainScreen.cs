using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Submariner.Screens
{
	public class MainScreen : Screen
	{
		private Texture2D _texture;

		public override bool IsActive { get; set; }

		public override void LoadContent(ContentManager content)
		{
			_texture = content.Load<Texture2D>("main");
		}

		public override void UnloadContent()
		{
			_texture.Dispose();
		}

		public override void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			var keyboardState = Keyboard.GetState(PlayerIndex.One);
			if (keyboardState.IsKeyDown(Keys.Escape))
			{
				RaiseChangeScreen("");
				return;
			}
			if (keyboardState.GetPressedKeys().Length > 0)
				RaiseChangeScreen("Game");
		}

		public override void Draw(GameTime gameTime)
		{
			Game.EngineInstance.ClearGraphicsDevice(Color.CornflowerBlue);
			Game.EngineInstance.DefaultSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			Game.EngineInstance.DefaultSpriteBatch.Draw(
				_texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
				Game.EngineInstance.Scale, SpriteEffects.None, 0f);
			Game.EngineInstance.DefaultSpriteBatch.End();
		}
	}
}
