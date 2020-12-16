using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Submariner.Particles;

namespace Submariner.Vessels
{
	public class DestroyerMine : ISprite
	{
		public static readonly Vector2 MoveVector = new Vector2(.08f, .5f);

		private readonly Vector2 _startPosition;
		private readonly float _power;
		private readonly float _startOffset;
		private Vector2 _trajectory;
		private readonly float _trajectoryChange;
		private readonly Explosion _explosion;

		public DestroyerMine(Destroyer destroyer, float power)
		{
			IsActive = true;
			_power = power;
			_startPosition = new Vector2(destroyer.Position.X + (_power >= 0 ? destroyer.GetWidth() : 0), destroyer.Position.Y + destroyer.GetHeight() * .71f);
			_trajectoryChange = destroyer.Position.Y + destroyer.GetHeight();
			_startOffset = destroyer.GetWidth() * .1f;
			_explosion = new Explosion();
		}

		public bool IsActive { get; set; }

		public Texture2D Texture { get; private set; }

		public Vector2 Position
		{
			get { return  new Vector2(_startPosition.X + _trajectory.X - (_power >= 0 ? 1 : -1) * _startOffset, _startPosition.Y + _trajectory.Y); }
		}

		public Color[,] Colors { get; private set; }

		public bool Exploding
		{
			get { return _explosion.Exploding; }
		}

		public void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("Vessels/mine");
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

			if (Position.Y > Game.EngineInstance.ScreenHeight)
				return;

			if (_power >= 0)
			{
				if (Position.Y <= _trajectoryChange)
				{
					_trajectory.Y = (.006f / _power) * _trajectory.X * _trajectory.X - _trajectory.X;
					_trajectory.X += 2f;
				}
				else
				{
					_trajectory.Y += MoveVector.Y;
					_trajectory.X += MoveVector.X;
				}
			}
			else
			{
				if (Position.Y <= _trajectoryChange)
				{
					_trajectory.Y = (.006f / -_power) * _trajectory.X * _trajectory.X + _trajectory.X;
					_trajectory.X -= 2f;
				}
				else
				{
					_trajectory.Y += MoveVector.Y;
					_trajectory.X -= MoveVector.X;
				}
			}
		}

		public void Draw(GameTime gameTime)
		{
			_explosion.Draw(gameTime);

			if (!IsActive)
				return;

			Game.EngineInstance.DefaultSpriteBatch.Draw(Texture, Position,
				null, Color.IndianRed, 0f, Vector2.Zero,
				Game.EngineInstance.GetScale(), SpriteEffects.None, 0f);

			//var scale = Game.EngineInstance.GetScale();
			//var vector = new Vector2(0, 0);
			//var trajectoryEnd = Game.EngineInstance.ScreenHeight;
			//if (_power >= 0)
			//{
			//    while (_startPosition.Y + vector.Y <= trajectoryEnd)
			//    {
			//        if (_startPosition.Y + vector.Y <= _trajectoryChange)
			//            vector.Y = .006f * vector.X * vector.X - vector.X;
			//        else
			//            vector.Y += 0.1f;

			//        Game.EngineInstance.DefaultSpriteBatch.Draw(Texture,
			//            new Vector2(_startPosition.X + vector.X - _startOffset, _startPosition.Y + vector.Y),
			//            null, Color.IndianRed, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			//        vector.X += .01f;
			//    }
			//    return;
			//}
			//while (_startPosition.Y + vector.Y <= trajectoryEnd)
			//{
			//    if (_startPosition.Y + vector.Y <= _trajectoryChange)
			//        vector.Y = .006f * vector.X * vector.X + vector.X;
			//    else
			//        vector.Y += 0.1f;

			//    Game.EngineInstance.DefaultSpriteBatch.Draw(Texture,
			//        new Vector2(_startPosition.X + vector.X + _startOffset, _startPosition.Y + vector.Y),
			//        null, Color.IndianRed, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			//    vector.X -= .01f;
			//}
		}

		public void Explode()
		{
			_explosion.Create(Position, 200, 5f, 10f, 110f, 2000f);
		}
	}
}
