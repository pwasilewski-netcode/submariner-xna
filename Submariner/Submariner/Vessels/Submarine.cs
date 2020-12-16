using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Submariner.Agents;

namespace Submariner.Vessels
{
	public class Submarine : ISprite
	{
		private const double ShootsTimeout = 5d;
		private const double MinTimeSpeed = 1;
		private const double MinTimeTurbo = 1;
		private const double Time = .0166667d;

		private float _top;
		//private readonly Random _random = new Random();
		private readonly int _frontSize;
		private readonly int _rearSize;
		private readonly Color _color;
		private readonly bool _toLeft;
		private Vector2 _position;
		private double _timeSpeed;
		private double _timeTurbo;
		private double _timeShoot;
		private int _turn;

		private Texture2D _bowsTexture;
		private Texture2D _frontTexture;
		private Texture2D _houseTexture;
		private Texture2D _rearTexture;
		private Texture2D _sternTexture;
		private SoundEffect _sonarSound;

		private readonly Enemies _enemies;

		public bool ToLeft
		{
			get { return _toLeft; }
		}

		public Submarine(Enemies enemies, AgentFactory agentFactory, bool toLeft, float speed, float top, int frontSize, int rearSize, Color color)
		{
			_enemies = enemies;
			Agent = agentFactory.CreateAgent(this);
			//Shoots = new[] { (float)_random.NextDouble(), (float)_random.NextDouble() };
			IsActive = true;
			MinSpeed = speed;
			MaxSpeed = speed * 4f;
			_toLeft = toLeft;
			_movementVector = new Vector2(toLeft ? -1f : 1f, 0f);
			_top = top;
			_frontSize = frontSize;
			_rearSize = rearSize;
			_color = color;
		}

		public Agent Agent { get; private set; }

		public bool IsActive { get; set; }

		// ReSharper disable UnusedAutoPropertyAccessor.Local
		public Texture2D Texture { get; private set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Local

		public Vector2 Position
		{
			get { return _position; }
		}

		private static float GetSpeed(double timeSpeed)
		{
			if (timeSpeed <= MinTimeSpeed)
				return 0;

			var speed = (float)Math.Log10(timeSpeed);
			return speed > 0 ? speed : 0;
		}

		private float GetTurbo(double timeTurbo)
		{
			if (timeTurbo <= MinTimeTurbo)
				return 1;

			var turbo = (float)Math.Log10(timeTurbo);
			if (turbo > MaxSpeed)
				return MaxSpeed;

			return MinSpeed + turbo;
		}

		//public bool ToLeft { get; set; }
		private Vector2 _movementVector;

		public Vector2 MovementVector
		{
			get { return _movementVector; }
		}

		public void Move(Vector2 vector2)
		{
			_movementVector += vector2;
			if (_movementVector.X > 1f)
				_movementVector.X = 1f;
			else if (_movementVector.X < -1f)
				_movementVector.X = -1f;

			if (_movementVector.Y > 1f)
				_movementVector.Y = 1f;
			else if (_movementVector.Y < -1f)
				_movementVector.Y = -1f;
		}

		public Color[,] Colors { get; private set; }

		//public float[] Shoots { get; set; }

		public void LoadContent(ContentManager content)
		{
			_sonarSound = content.Load<SoundEffect>("Vessels/sonar");
			_bowsTexture = content.Load<Texture2D>("Vessels/submarinebows");
			_frontTexture = content.Load<Texture2D>("Vessels/submarinefront");
			_houseTexture = content.Load<Texture2D>("Vessels/submarinehouse");
			_rearTexture = content.Load<Texture2D>("Vessels/submarinerear");
			_sternTexture = content.Load<Texture2D>("Vessels/submarinestern");

			Texture = content.Load<Texture2D>("Vessels/submarine");
			_top -= Texture.Height / Game.EngineInstance.Y;
			if (_toLeft)
			{
				Colors = Game.Engine.TextureTo2DArray(Texture);
			}
			else
			{
				var colors = Game.Engine.TextureTo2DArray(Texture);
				var width = colors.GetLength(0);
				var height = colors.GetLength(1);
				Colors = new Color[width, height];
				for (var x = 0; x < width; x++)
				{
					for (var y = 0; y < height; y++)
					{
						Colors[x, y] = colors[width - 1 - x, y];
					}
				}
			}

			//Texture = Game.EngineInstance.CreateTexture2D(
			//    _bowsTexture.Width + _frontSize * _frontTexture.Width + _houseTexture.Width
			//        + _rearSize * _rearTexture.Width + _sternTexture.Width,
			//    _bowsTexture.Height + _frontTexture.Height + _houseTexture.Height
			//        + _rearTexture.Height + _sternTexture.Height,
			//    Color.Transparent);

			//var offset = 0;
			//var colors = Game.Engine.GetColorsFromTexture(_bowsTexture);
			//Texture.SetData(colors, offset, colors.Length);
			//offset += colors.Length;

			//colors = Game.Engine.GetColorsFromTexture(_frontTexture);
			//for (var i = 0; i < _frontSize; i++)
			//{
			//    Texture.SetData(colors, offset, colors.Length);
			//    offset += colors.Length;
			//}

			//colors = Game.Engine.GetColorsFromTexture(_houseTexture);
			//Texture.SetData(colors, offset, colors.Length);
			//offset += colors.Length;

			//colors = Game.Engine.GetColorsFromTexture(_rearTexture);
			//for (var i = 0; i < _rearSize; i++)
			//{
			//    Texture.SetData(colors, offset, colors.Length);
			//    offset += colors.Length;
			//}

			//colors = Game.Engine.GetColorsFromTexture(_sternTexture);
			//Texture.SetData(colors, offset, colors.Length);

			_position = new Vector2(_toLeft ? Game.EngineInstance.ScreenWidth : -GetWidth(), Game.EngineInstance.ScreenHeight * _top);
		}

		public void UnloadContent()
		{
			IsActive = false;
			_sternTexture.Dispose();
			_rearTexture.Dispose();
			_houseTexture.Dispose();
			_frontTexture.Dispose();
			_bowsTexture.Dispose();
		}

		public void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			if (_timeShoot < ShootsTimeout / _enemies.Level)
				_timeShoot += Time;

			Agent.Update(gameTime);
			UpdateMovement(ref _position, ref _timeSpeed, ref _timeTurbo, ref _turn, _movementVector);
		}

		public void Draw(GameTime gameTime)
		{
			if (!IsActive)
				return;

			Game.EngineInstance.DefaultSpriteBatch.Draw(
				Texture, Position, null, _color, 0f,
				Vector2.Zero, Game.EngineInstance.GetScale(),
				_toLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

			//if (_toLeft)
			//    DrawToLeft();
			//else
			//    DrawToRight();
		}

		//private void DrawToLeft()
		//{
		//    var offset = 0f;
		//    var scaleX = Game.EngineInstance.GetScale().X;

		//    DrawPart(offset, _bowsTexture);
		//    offset += _bowsTexture.Width * scaleX;

		//    for (var i = 0; i < _frontSize; i++)
		//    {
		//        DrawPart(offset, _frontTexture);
		//        offset += _frontTexture.Width * scaleX;
		//    }

		//    DrawPart(offset, _houseTexture);
		//    offset += _houseTexture.Width * scaleX;

		//    for (var i = 0; i < _rearSize; i++)
		//    {
		//        DrawPart(offset, _rearTexture);
		//        offset += _rearTexture.Width * scaleX;
		//    }

		//    DrawPart(offset, _sternTexture);
		//}

		//private void DrawToRight()
		//{
		//    var offset = 0f;
		//    var scaleX = Game.EngineInstance.GetScale().X;

		//    DrawPart(offset, _sternTexture);
		//    offset += _sternTexture.Width * scaleX;

		//    for (var i = 0; i < _rearSize; i++)
		//    {
		//        DrawPart(offset, _rearTexture);
		//        offset += _rearTexture.Width * scaleX;
		//    }

		//    DrawPart(offset, _houseTexture);
		//    offset += _houseTexture.Width * scaleX;

		//    for (var i = 0; i < _frontSize; i++)
		//    {
		//        DrawPart(offset, _frontTexture);
		//        offset += _frontTexture.Width * scaleX;
		//    }

		//    DrawPart(offset, _bowsTexture);
		//}

		//private void DrawPart(float offset, Texture2D texture2D)
		//{
		//    var position = new Vector2(Position.X + offset, Position.Y);
		//    Game.EngineInstance.DefaultSpriteBatch.Draw(
		//        texture2D, position, null, _color, 0f,
		//        Vector2.Zero, Game.EngineInstance.GetScale(),
		//        ToLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
		//}

		public Rectangle GetRectangle()
		{
			return new Rectangle((int)_position.X, (int)_position.Y, (int)GetWidth(), (int)GetHeight());
		}

		public float GetWidth()
		{
			var scaleX = Game.EngineInstance.GetScale().X;
			return scaleX * _bowsTexture.Width + scaleX * _frontTexture.Width * _frontSize + scaleX * _houseTexture.Width + scaleX * _rearTexture.Width * _rearSize + scaleX * _sternTexture.Width;
		}

		public float GetHeight()
		{
			return Game.EngineInstance.GetScale().Y * _bowsTexture.Height;
		}

		public bool IsOutOfBoard()
		{
			if (_movementVector.X < 0)
				return _position.X < -GetWidth();

			return _movementVector.X >= 0 && _position.X > Game.EngineInstance.ScreenWidth;
		}

		public float MinSpeed { get; set; }

		public float MaxSpeed { get; set; }

		public void PlaySonarSound(float distance)
		{
			if (_sonarSound.IsDisposed)
				return;

			if (distance < 0f)
				distance = 0f;
			else if (distance > 1f)
				distance = 1f;
			var pan = (2f * _position.X / Game.EngineInstance.ScreenWidth) - 1;
			if (pan < -1f)
				pan = -1f;
			else if (pan > 1f)
				pan = 1f;
			_sonarSound.Play(.4f * distance, 0f, pan);
		}

		public void UseTurbo()
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

		private void UpdateMovement(ref Vector2 position, ref double timeSpeed, ref double timeTurbo, ref int turn, Vector2 vector)
		{
			if (timeTurbo > 0)
			{
				timeTurbo -= Time;
				if (timeTurbo < 0)
					timeTurbo = 0;
			}

			var speedBreak = MaxSpeed * 4;
			if (speedBreak > 6)
				speedBreak = 6;
			if (vector.X < 0)
			{
				if (turn > 0)
				{
					timeSpeed -= speedBreak * Time;
					if (timeSpeed < MinTimeSpeed)
						turn = -1;
				}
				else
				{
					if (timeSpeed > MaxSpeed)
						timeSpeed = MaxSpeed;
					else
						timeSpeed += Time;
					turn = -1;
				}
			}
			else if (vector.X > 0)
			{
				if (turn < 0)
				{
					timeSpeed -= speedBreak * Time;
					if (timeSpeed < MinTimeSpeed)
						turn = 1;
				}
				else
				{
					if (timeSpeed > MaxSpeed)
						timeSpeed = MaxSpeed;
					else
						timeSpeed += Time;
					turn = 1;
				}
			}
			else
			{
				if (timeSpeed > MinTimeSpeed)
				{
					timeSpeed -= Time;
					if (timeSpeed < MinTimeSpeed)
						timeSpeed = MinTimeSpeed;
				}
			}

			position.X += GetSpeed(timeSpeed) * GetTurbo(timeTurbo) * turn * Math.Abs(vector.X);
		}

		public float GetTimeToDestination(Vector2 destination, Vector2 vector)
		{
			var time = 0d;
			var timeSpeed = _timeSpeed;
			var timeTurbo = _timeTurbo;
			var turn = _turn;
			var position = _position;
			//var toLeft = destination.X < position.X;
			//while ((turn < 0 && destination.X <= position.X) || (turn > 0 && destination.X >= position.X))
			while ((vector.X < 0 && destination.X <= position.X) || (vector.X > 0 && destination.X >= position.X))
			{
				UpdateMovement(ref position, ref timeSpeed, ref timeTurbo, ref turn, vector);
				time += Time;
			}
			//while (Math.Abs(_position.X - position.X) < .01f);

			return (float)time;
		}

		public float GetDestroyerMineTimeToBlast(DestroyerMine destroyerMine)
		{
			var time = 0d;
			var position = destroyerMine.Position;
			while (_position.Y >= position.Y)
			{
				position += DestroyerMine.MoveVector;
				time += Time;
			}
			//while (Math.Abs(_position.X - position.X) < .01f);
			
			return (float)time;
		}

		public void Shoot()
		{
			//if (_timeShoot < ShootsTimeout / _enemies.Level)
			//    return;

			//_timeShoot = 0d;
			//_enemies.Shoot(this);
		}
	}
}
