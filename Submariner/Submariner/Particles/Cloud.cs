using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Submariner.Particles
{
	public class Cloud : ISprite
	{
		private SpriteBatch _spriteBatch;

		public Cloud(Vector2 position, int particleCount, float size, float scaleFactor)
		{
			IsActive = true;
			Position = position;
		}

		public bool IsActive { get; set; }
	
		public Texture2D Texture { get; private set; }
		
		public Vector2 Position { get; private set; }
		
		public void LoadContent(ContentManager content)
		{
			_spriteBatch = Game.EngineInstance.CreateSpriteBatch();
			Texture = content.Load<Texture2D>("Particles/cloud");
		}

		public void UnloadContent()
		{
			Texture.Dispose();
			_spriteBatch.Dispose();
		}

		public void Update(GameTime gameTime)
		{
		}

		public void Draw(GameTime gameTime)
		{
			if (!IsActive)
				return;

			//_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			//_spriteBatch.Draw(Texture, Position, Color.Black);
			//_spriteBatch.End();
			Game.EngineInstance.DefaultSpriteBatch.Draw(Texture, Position, Color.White);
		}
	}
}
