using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Submariner.Particles;

namespace Submariner.Environment
{
	public class Sky : ISprite
	{
		private readonly Cloud[] _clouds = new []
		{
			new Cloud(new Vector2(100, 20), 50, 20f, 1f),
		};

		public Sky()
		{
			IsActive = true;
			Position = Vector2.Zero;
		}

		public bool IsActive { get; set; }

		public Texture2D Texture { get; private set; }

		public Vector2 Position { get; private set; }

		public void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("Environment/sky");
			Array.ForEach(_clouds, c => c.LoadContent(content));
		}

		public void UnloadContent()
		{
			Array.ForEach(_clouds, c => c.UnloadContent());
			Texture.Dispose();
		}

		public void Update(GameTime gameTime)
		{
			Array.ForEach(_clouds, c => c.Update(gameTime));
		}

		public void Draw(GameTime gameTime)
		{
			if (!IsActive)
				return;

			Game.EngineInstance.DefaultSpriteBatch.Draw(
				Texture, Position, null, Color.White, 0f, Vector2.Zero,
				Game.EngineInstance.Scale, SpriteEffects.None, 0f);

			//Game.EngineInstance.Draw(this);
			//Array.ForEach(_clouds, c => c.Draw(gameTime));
		}
	}
}
