using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Submariner.Environment
{
	public class Wave : ISprite
	{
		private readonly float _top;
		private readonly float _stepX;
		private readonly float _oscilateY;
		private readonly float _stepY;
		private readonly Curve _curveY;
		private float[] _curve;
		private float[] _curveTrend;
		private float[] _curveMax;
		private Texture2D _texture2D;

		public Wave(float top, float stepX, float oscilateY, float stepY)
		{
			_top = top;
			_stepX = stepX;
			_oscilateY = oscilateY;
			_stepY = stepY;
			IsActive = true;
			_curveY = new Curve { PreLoop = CurveLoopType.Oscillate, PostLoop = CurveLoopType.Oscillate };
		}

		public bool IsActive { get; set; }

		public Texture2D Texture { get; private set; }

		public Vector2 Position { get; private set; }

		public void LoadContent(ContentManager content)
		{
			Texture = content.Load<Texture2D>("Environment/wave");
			_texture2D = Game.EngineInstance.CreateTexture2D(Texture.Width, (int)((Texture.Height / Game.EngineInstance.Y) * Game.EngineInstance.ScreenHeight), Color.White);

			Position = new Vector2(0, Game.EngineInstance.ScreenHeight * _top);

			var length = (Game.EngineInstance.ScreenWidth / (int)_stepX) + 2;

			_curve = new float[length];
			_curveMax = new float[length];
			_curveTrend = new float[length];

			var random = new Random();
			var y = _oscilateY;

			for (var i = 0; i < _curve.Length; i++)
			{
				_curveMax[i] = ((float)random.NextDouble()) * (y *= -1);
				_curve[i] = _curveMax[i];
				_curveTrend[i] = random.NextDouble() > .5d ? 1f : -1f;
			}
		}

		public void UnloadContent()
		{
			_texture2D.Dispose();
			Texture.Dispose();
		}

		public void Update(GameTime gameTime)
		{
			_curveY.Keys.Clear();
			for (var i = 0; i < _curve.Length; i++)
			{
				if (_curve[i] > _oscilateY || _curve[i] < -_oscilateY)
					_curveTrend[i] *= -1f;

				_curve[i] += _stepY * _curveTrend[i];
				_curveY.Keys.Add(new CurveKey(i * _stepX, Position.Y + (_curve[i])));
			}
			_curveY.ComputeTangents(CurveTangent.Smooth);
		}

		public float Evaluate(float x)
		{
			return _curveY.Evaluate(x);
		}

		public void Draw(GameTime gameTime)
		{
			if (!IsActive)
				return;

			var height = (int)((Texture.Height / Game.EngineInstance.Y) * Game.EngineInstance.ScreenHeight);
			for (var x = 0f; x <= Game.EngineInstance.ScreenWidth; x += 1f)
			{
				//Position = new Vector2(_curveX.Evaluate(i), _curveY.Evaluate(i));
				//Game.EngineInstance.Draw(this, Color.CornflowerBlue);
				//Game.EngineInstance.DefaultSpriteBatch.Draw(Texture, new Vector2(_curveX.Evaluate(i), _curveY.Evaluate(i)), Color.CornflowerBlue);
				var y = Evaluate(x);
				//Game.EngineInstance.DefaultSpriteBatch.Draw(_texture2D, new Vector2(x, y), Color.Blue);
				Game.EngineInstance.DefaultSpriteBatch.Draw(Texture, new Rectangle((int)x, (int)y, Texture.Width, height), Color.LightBlue);
			}
		}
	}
}
