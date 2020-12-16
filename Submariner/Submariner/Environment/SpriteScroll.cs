using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Submariner.Environment
{
	public class SpriteScroll : ISprite
	{
		private readonly string _textureName;
		private readonly Color _color;
		private Vector2[] _positions;

		public SpriteScroll(float speed, string textureName, Color color)
		{
			_textureName = textureName;
			_color = color;
			IsActive = true;
			Speed = speed;
		}

		public float Speed { get; set; }

		public virtual bool IsActive { get; set; }
		
		public Texture2D Texture { get; private set; }
		
		public Vector2 Position { get; private set; }
		
		public virtual void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>(_textureName);
			var width = Game.EngineInstance.ScreenWidth;
			_positions = new Vector2[2];
			for (var i = 0; i < _positions.Length; i++)
			{
				_positions[i] = new Vector2(i * width, 0);
			}
		}

		public virtual void UnloadContent()
		{
			Texture.Dispose();
		}

		public virtual void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			var width = Game.EngineInstance.ScreenWidth;
			for (var i = 0; i < _positions.Length; i++)
			{
				_positions[i].X += Speed;
				if (Speed <= 0)
				{
					if (_positions[i].X <= -width)
					{
						_positions[i].X = width * (_positions.Length - 1);
					}
				}
				else
				{
					if (_positions[i].X >= width)
					{
						_positions[i].X -= width * _positions.Length;
					}
				}
			}
		}

		public virtual void Draw(GameTime gameTime)
		{
			if (!IsActive)
				return;

			foreach (var position in _positions)
			{
				Position = position;
				Game.EngineInstance.DefaultSpriteBatch.Draw(
					Texture, Position, null, _color, 0f, Vector2.Zero,
					Game.EngineInstance.Scale, SpriteEffects.None, 0f);

				//Game.EngineInstance.Draw(this, _color);
			}
		}
	}
}
