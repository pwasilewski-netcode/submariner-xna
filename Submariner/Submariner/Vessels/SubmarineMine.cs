using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Submariner.Environment;
using Submariner.Particles;

namespace Submariner.Vessels
{
	public class SubmarineMine : ISprite
	{
		private const float WaveScale = .09f;

		private Vector2 _position;
		private readonly float _power;
		private readonly float _top;
		private readonly Wave _wave;
		private readonly Explosion _explosion;

		public SubmarineMine(Submarine submarine, float top, Wave wave)
		{
			IsActive = true;
			_power = submarine.MaxSpeed / 10;
			_position = new Vector2(submarine.Position.X + (submarine.Position.X < 0f ? 0 : submarine.GetWidth()), submarine.Position.Y + submarine.GetHeight() * .8f);
			_top = Game.EngineInstance.ScreenHeight * top;
			_wave = wave;
			_explosion = new Explosion();
		}

		public bool IsActive { get; set; }

		public Texture2D Texture { get; private set; }

		public Vector2 Position
		{
			get { return _position; }
		}

		public Color[,] Colors { get; private set; }

		public bool IsTop { get; private set; }

		public bool Exploding
		{
			get { return _explosion.Exploding; }
		}

		public void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("Vessels/submarinemine");
			Colors = Game.Engine.TextureTo2DArray(Texture);
			_explosion.LoadContent(content);
		}

		public void UnloadContent()
		{
			IsActive = false;
			_explosion.IsActive = false;
			_explosion.UnloadContent();
			//Texture.Dispose();
		}

		public void Update(GameTime gameTime)
		{
			_explosion.Update(gameTime);

			if (!IsActive)
				return;

			if (_position.Y <= _top)
			{
				IsTop = true;
				var offset = (_wave.Evaluate(_position.X + (Game.EngineInstance.ScaleTexture(Texture).X / 2f)) - _wave.Position.Y) * WaveScale;
				//var oldPositionY = _position.Y;
				_position.Y -= offset;
				return;
			}

			_position.Y -= _power;
		}

		public void Draw(GameTime gameTime)
		{
			_explosion.Draw(gameTime);

			if (!IsActive)
				return;

			Game.EngineInstance.DefaultSpriteBatch.Draw(Texture, _position,
				null, Color.Gray, 0f, Vector2.Zero,
				Game.EngineInstance.GetScale(), SpriteEffects.None, 0f);
		}

		public void Explode()
		{
			_explosion.Create(Position, 200, 5f, 10f, 110f, 2000f);
		}
	}
}
