using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Submariner.Environment;
using Submariner.Particles;

namespace Submariner.Vessels
{
	public class Destroyer : ISprite
	{
		private const float WaveScale = .09f;
		private const double MinTimeSpeed = 1;
		private const double MinTimeTurbo = 1;

		private readonly float _top;

		private Vector2 _position;
		private double _timeSpeed;
		private double _timeTurbo;
		private readonly float _shootPowerStep;
		private bool _stopShooting;
		private int _turn;
		private float _rotation;
		private KeyboardState _keyboardState = Keyboard.GetState(PlayerIndex.One);
		private readonly Explosion _explosion;

		public Destroyer(float top)
		{
			Mines = new List<DestroyerMine>();
			_position = Vector2.Zero;
			_top = top;
			IsActive = true;
			MinSpeed = 2;
			MaxSpeed = 4;
			_shootPowerStep = .8f;
			_explosion = new Explosion();
		}

		private bool _isActive;
		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				_isActive = value;
				if (!_isActive)
				{
					Mines.ForEach(m => m.UnloadContent());
					Mines.Clear();
				}
			}
		}

		public Texture2D Texture { get; private set; }

		public Vector2 Position
		{
			get { return _position; }
		}

		public float MinSpeed { get; set; }
		
		public float MaxSpeed { get; set; }
		
		public float Speed
		{
			get
			{
				if (_timeSpeed <= MinTimeSpeed)
					return 0;

				var speed = (float)Math.Log10(_timeSpeed);
				return speed > 0 ? speed : 0;
			}
		}

		public float Turbo
		{
			get
			{
				if (_timeTurbo <= MinTimeTurbo)
					return 1;

				var turbo = (float)Math.Log10(_timeTurbo);
				if (turbo > MaxSpeed)
					return MaxSpeed;

				return MinSpeed + turbo;
			}
		}

		public float Power { get; private set; }

		public List<DestroyerMine> Mines { get; private set; }

		public Color[,] Colors { get; private set; }

		public void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("Vessels/destroyer");
			_position.X = ((float)Game.EngineInstance.ScreenWidth / 2) - (Game.EngineInstance.ScaleTexture(Texture).X / 2);
			_position.Y = Game.EngineInstance.ScreenHeight * _top;
			Colors = Game.Engine.TextureTo2DArray(Texture);
			_explosion.LoadContent(content);
		}

		public void UnloadContent()
		{
			Texture.Dispose();
			_explosion.IsActive = false;
			_explosion.UnloadContent();
		}

		public void Update(GameTime gameTime)
		{
			_explosion.Update(gameTime);

			if (!IsActive)
				return;

			var keyboardState = Keyboard.GetState(PlayerIndex.One);
			UpdateMovement(gameTime, keyboardState);
			UpdateTurbo(gameTime, keyboardState);
			UpdateShooting(gameTime, keyboardState);
			_position.X += Speed * Turbo * _turn;
			
			foreach (var mine in Mines)
				mine.Update(gameTime);

			_keyboardState = keyboardState;
		}

		private void UpdateShooting(GameTime gameTime, KeyboardState keyboardState)
		{
			var powerStep = (float)gameTime.ElapsedGameTime.TotalSeconds * _shootPowerStep;
			if (_stopShooting)
			{
				Power -= 4 * powerStep;
				if (Power <= 0f)
				{
					Power = 0f;
					_stopShooting = false;
				}
			}
			else
			{
				if (keyboardState.IsKeyDown(Keys.Z))
				{
					Power += powerStep;
					if (Power > 1f)
						Power = 1f;
				}
				else if (Power > 0f && _keyboardState.IsKeyDown(Keys.Z) && keyboardState.IsKeyUp(Keys.Z))
				{
					Shoot(-Power);
					_stopShooting = true;
				}
				else if (keyboardState.IsKeyDown(Keys.X))
				{
					Power += powerStep;
					if (Power > 1f)
						Power = 1f;
				}
				else if (Power > 0f && _keyboardState.IsKeyDown(Keys.X) && keyboardState.IsKeyUp(Keys.X))
				{
					Shoot(Power);
					_stopShooting = true;
				}
			}
		}

		private void UpdateTurbo(GameTime gameTime, KeyboardState keyboardState)
		{
			_timeTurbo = MaxSpeed;
			//if (keyboardState.IsKeyDown(Keys.LeftShift))
			//{
			//    if (_timeTurbo > MaxSpeed)
			//        _timeTurbo = MaxSpeed;
			//    else
			//        _timeTurbo += gameTime.ElapsedGameTime.TotalSeconds;
			//}
			//else
			//{
			//    if (_timeTurbo > MinTimeTurbo)
			//    {
			//        _timeTurbo -= gameTime.ElapsedGameTime.TotalSeconds;
			//        if (_timeTurbo < MinTimeTurbo)
			//            _timeTurbo = MinTimeTurbo;
			//    }
			//}
		}

		private void UpdateMovement(GameTime gameTime, KeyboardState keyboardState)
		{
			var speedBreak = MaxSpeed * 4;
			if (speedBreak > 6)
				speedBreak = 6;
			if (keyboardState.IsKeyDown(Keys.Left))
			{
				if (_turn > 0)
				{
					_timeSpeed -= speedBreak * gameTime.ElapsedGameTime.TotalSeconds;
					if (_timeSpeed < MinTimeSpeed)
						_turn = -1;
				}
				else
				{
					if (_timeSpeed > MaxSpeed)
						_timeSpeed = MaxSpeed;
					else
						_timeSpeed += gameTime.ElapsedGameTime.TotalSeconds;
					_turn = -1;
				}
			}
			else if (keyboardState.IsKeyDown(Keys.Right))
			{
				if (_turn < 0)
				{
					_timeSpeed -= speedBreak * gameTime.ElapsedGameTime.TotalSeconds;
					if (_timeSpeed < MinTimeSpeed)
						_turn = 1;
				}
				else
				{
					if (_timeSpeed > MaxSpeed)
						_timeSpeed = MaxSpeed;
					else
						_timeSpeed += gameTime.ElapsedGameTime.TotalSeconds;
					_turn = 1;
				}
			}
			else
			{
				if (_timeSpeed > MinTimeSpeed)
				{
					_timeSpeed -= gameTime.ElapsedGameTime.TotalSeconds;
					if (_timeSpeed < MinTimeSpeed)
						_timeSpeed = MinTimeSpeed;
				}
			}
		}

		public void Shoot(float power)
		{
			if (Mines.Count >= 10)
				return;

			var mine = new DestroyerMine(this, power);
			mine.LoadContent(Game.EngineInstance.Content);
			Mines.Add(mine);
		}

		public void WaveOffset(Wave wave)
		{
			if (!IsActive)
				return;

			var offset = (wave.Evaluate(_position.X + (Game.EngineInstance.ScaleTexture(Texture).X / 2f)) - wave.Position.Y) * WaveScale;
			//var oldPositionY = _position.Y;
			_position.Y -= offset;
			_rotation = -offset / (float)(Math.PI * 3);
		}

		public void Draw(GameTime gameTime)
		{
			_explosion.Draw(gameTime);

			if (!IsActive)
				return;

			var scale = Game.EngineInstance.ScaleTexture(Texture);
			//var origin = new Vector2(scale.X / 2f, 0);
			Game.EngineInstance.DefaultSpriteBatch.Draw(
				Texture,
				new Rectangle((int)(Position.X), (int)Position.Y, (int)scale.X, (int)scale.Y),
				null,
				Color.White,
				_rotation,
				Vector2.Zero/*origin*/,
				SpriteEffects.None,
				0f);

			foreach (var mine in Mines)
				mine.Draw(gameTime);
		}

		public float GetWidth()
		{
			return Game.EngineInstance.GetScale().X * Texture.Width;
		}

		public float GetHeight()
		{
			return Game.EngineInstance.GetScale().Y * Texture.Height;
		}

		public void Explode(SubmarineMine submarineMine)
		{
			_explosion.Create(submarineMine.Position, 200, 5f, 15f, 100f, 2000f);
		}
	}
}
