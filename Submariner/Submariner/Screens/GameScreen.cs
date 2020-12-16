using System;
//using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Submariner.Environment;
//using Submariner.Particles;
using Submariner.Vessels;
using System.Linq;

namespace Submariner.Screens
{
	public class GameScreen : Screen
	{
		private const int ScoreMine = 150;
		private const int ScoreSubmarine = 200;
		private const int ScoreMiss = -100;
		private const int ScoreDestroy = -200;
		private const int ScoreToLiveUp = 5000;
		private const int ScoreToNextLevel = 1000;

		private long _scoreToLiveUp;
		private long _scoreToNextLevel;

		//private const float WaveTop = .32f;
		private const float WaveTop = .21f;
		private const float WaveTopOffset = .03f;
		//private const float DestroyerTopOffset = .141f;

		private Texture2D _textureGameOver;
		private Texture2D _textureNextLevel;

		private readonly Backland _backland = new Backland(-.06f);
		private readonly Sky _sky = new Sky();
		private readonly Sea _sea = new Sea(-.15f, Color.CornflowerBlue);
		private readonly Wave[] _waves = new[]
		{
			new Wave(WaveTop + 0 * WaveTopOffset, 70f, 1f, .03f),
			new Wave(WaveTop + 1 * WaveTopOffset, 75f, 5f, .05f),
			new Wave(WaveTop + 2 * WaveTopOffset, 80f, 5f, .06f),
			new Wave(WaveTop + 3 * WaveTopOffset, 90f, 2f, .09f),
			new Wave(WaveTop + 4 * WaveTopOffset, 99f, 2f, .07f),
		};
		private readonly Seabed _seabed = new Seabed(-.1f);
		private readonly Reef _reef = new Reef(-.25f);
		private readonly Destroyer _destroyer = new Destroyer(WaveTop - .95f * WaveTopOffset/* - 2.5f * WaveTopOffset*/);
		//private readonly Explosion _explosion = new Explosion();

		private Enemies _enemies;

		private bool _isActive;

		public override bool IsActive
		{
			get { return _isActive; }
			set
			{
				_isActive = value;
				if (_enemies != null)
					_enemies.IsActive = _isActive;
				_destroyer.IsActive = _isActive;
				_sea.IsActive = _isActive;
				if (_isActive)
				{
					Lives = 3;
					Score = 0;
				}
			}
		}

		public long Score { get; private set; }

		public int Lives { get; private set; }

		public override void LoadContent(ContentManager content)
		{
			_textureGameOver = content.Load<Texture2D>("gameover");
			_textureNextLevel = content.Load<Texture2D>("nextlevel");
			_backland.LoadContent(content);
			_sky.LoadContent(content);
			Array.ForEach(_waves, w => w.LoadContent(content));
			_seabed.LoadContent(content);
			_reef.LoadContent(content);
			_sea.LoadContent(content);
			_destroyer.LoadContent(content);
			_enemies = new Enemies(content, _destroyer, /*.5f*/WaveTop + 3f * WaveTopOffset, _waves[3]);
			//_explosion.LoadContent(content);
		}

		public override void UnloadContent()
		{
			//_explosion.UnloadContent();
			_enemies.Dispose();
			_destroyer.UnloadContent();
			_sea.UnloadContent();
			_seabed.UnloadContent();
			_reef.UnloadContent();
			_sky.UnloadContent();
			_backland.UnloadContent();
			_textureNextLevel.Dispose();
			_textureGameOver.Dispose();
		}

		public override void Update(GameTime gameTime)
		{
			var escape = Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape);
			if (IsActive && escape)
			{
				IsActive = false;
				Lives = -1;
			}
			else if (!IsActive && Lives < 0 && escape)
			{
				RaiseChangeScreen("Main");
			}

			//if (!IsActive)
			//    return;

			//if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Space))
				////_explosion.Create(new Vector2(256, 256), 4, 50f, 30f, 200f, 1000f);
				////_explosion.Create(new Vector2(256, 256), 10, 50f, 80f, 200f, 2000f);
				////_explosion.Create(new Vector2(256, 256), 100, 30f, 300f, 200f, 2000f);
				////_explosion.Create(new Vector2(256, 256), 200, 20f, 10f, 200f, 2000f);
				////_explosion.Create(new Vector2(256, 256), 200, 10f, 25f, 150f, 2000f);
				////_explosion.Create(new Vector2(256f, 256f), 200, 10f, 25f, 150f, 2500f);
				//_explosion.Create(new Vector2(256f, 256f), 200, 5f, 15f, 100f, 2000f);
				////_explosion.Create(new Vector2(256, 256), 300, 15f, 40f, 200f, 3000f);
			_sky.Update(gameTime);
			_backland.Update(gameTime);
			Array.ForEach(_waves, w => w.Update(gameTime));
			_seabed.Update(gameTime);
			_reef.Update(gameTime);
			_sea.Update(gameTime);
			//_explosion.Update(gameTime);
			_destroyer.Update(gameTime);
			_enemies.Update(gameTime);
			_destroyer.WaveOffset(_waves[3]);
			CheckCollisions();
		}

		private void CheckCollisions()
		{
			if (!IsActive)
				return;

			//var destroyerMinesToRemove = new List<DestroyerMine>();
			//var submarinesToRemove = new List<Submarine>();
			foreach (var submarine in _enemies.Submarines.Where(s => s.IsActive))
			{
				foreach (var destroyerMine in _destroyer.Mines.Where(m => m.IsActive && !m.Exploding))
				{
					if (Game.EngineInstance.Collides(destroyerMine.Colors, destroyerMine.Position, submarine.Colors, submarine.Position))
					{
						//destroyerMinesToRemove.Add(destroyerMine);
						//submarinesToRemove.Add(submarine);
						destroyerMine.IsActive = false;
						submarine.IsActive = false;
						//destroyerMine.UnloadContent();
						//submarine.UnloadContent();
						//_explosion.Create(destroyerMine.Position, 200, 5f, 15f, 100f, 2000f);
						destroyerMine.Explode();
						Score += ScoreSubmarine;
						_scoreToLiveUp += ScoreSubmarine;
						_scoreToNextLevel += ScoreSubmarine;
					}
				}
			}

			//var submarineMinesToRemove = new List<SubmarineMine>();
			foreach (var submarineMine in _enemies.Mines.Where(m => m.IsActive && !m.Exploding))
			{
				foreach (var destroyerMine in _destroyer.Mines.Where(m => m.IsActive && !m.Exploding))
				{
					if (Game.EngineInstance.Collides(destroyerMine.Colors, destroyerMine.Position, submarineMine.Colors, submarineMine.Position))
					{
						//destroyerMinesToRemove.Add(destroyerMine);
						destroyerMine.IsActive = false;
						//submarineMinesToRemove.Add(submarineMine);
						submarineMine.IsActive = false;
						//_explosion.Create(destroyerMine.Position, 200, 2f, 5f, 180f, 2000);
						submarineMine.Explode();
						destroyerMine.Explode();
						Score += ScoreMine;
						_scoreToLiveUp += ScoreMine;
						_scoreToNextLevel += ScoreMine;
					}
				}
			}

			foreach (var submarineMine in _enemies.Mines.Where((m => m.IsActive && !m.Exploding)))
			{
				if (Game.EngineInstance.Collides(submarineMine.Colors, submarineMine.Position, _destroyer.Colors, _destroyer.Position))
				{
					//submarineMinesToRemove.Add(submarineMine);
					submarineMine.IsActive = false;
					//_explosion.Create(submarineMine.Position, 200, 5f, 10f, 110f, 2000f);
					submarineMine.Explode();
					_destroyer.Explode(submarineMine);
					Score += ScoreDestroy;
					if (--Lives < 0)
						GameOver();
				}
			}

			foreach (var destroyerMine in _destroyer.Mines.Where(m => m.IsActive && !m.Exploding))
			{
				if (destroyerMine.Position.Y > Game.EngineInstance.ScreenHeight)
				{
					destroyerMine.IsActive = false;
					Score += ScoreMiss;
				}
			}

			if (Score < 0)
				Score = 0;

			if (_scoreToLiveUp >= ScoreToLiveUp)
			{
				Lives++;
				_scoreToLiveUp = 0;
			}
			if (_scoreToNextLevel >= ScoreToNextLevel)
			{
				_enemies.NextLevel();
				_scoreToNextLevel = 0;
			}

			//_enemies.Submarines.RemoveAll(submarinesToRemove.Contains);
			//_enemies.Mines.RemoveAll(submarineMinesToRemove.Contains);
			//_destroyer.Mines.RemoveAll(destroyerMinesToRemove.Contains);

			_enemies.Submarines.RemoveAll(s =>
			{
				if (!s.IsActive)
				{
					s.UnloadContent();
					return true;
				}
				return false;
			});
			_enemies.Mines.RemoveAll(m =>
			{
				if (!m.IsActive && !m.Exploding)
				{
					m.UnloadContent();
					return true;
				}
				return false;
			});
			_destroyer.Mines.RemoveAll(m =>
			{
				if (!m.IsActive && !m.Exploding)
				{
					m.UnloadContent();
					return true;
				}
				return false;
			});
		}

		public override void Draw(GameTime gameTime)
		{
			//if (!IsActive)
			//    return;

			Game.EngineInstance.ClearGraphicsDevice(Color.CornflowerBlue);
			Game.EngineInstance.DefaultSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			_sky.Draw(gameTime);
			_backland.Draw(gameTime);
			_seabed.Draw(gameTime);
			_enemies.DrawSubmarines(gameTime);
			_enemies.DrawMinesBack(gameTime);
			_reef.Draw(gameTime);
			//_explosion.Draw(gameTime);
			_sea.Draw(gameTime);
			DrawWaves(gameTime, 0, 2);
			_destroyer.Draw(gameTime);
			_enemies.DrawMinesFront(gameTime);
			DrawWaves(gameTime, 3, 4);
			//Game.EngineInstance.DrawString(_destroyer.Power.ToString(), new Vector2(100, 0), Color.Yellow);

			var lives = string.Concat("Lives: ", (Lives < 0 ? 0 : Lives).ToString());
			Game.EngineInstance.DefaultSpriteBatch.DrawString(
				Game.EngineInstance.DefaultFont, lives, new Vector2(10, 10),
				Color.LightGoldenrodYellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			var score = Score.ToString("D9");
			var textSize = Game.EngineInstance.DefaultFont.MeasureString(score);
			var drawPosition = new Vector2(Game.EngineInstance.ScreenWidth - textSize.X - 10, 10);
			Game.EngineInstance.DefaultSpriteBatch.DrawString(
				Game.EngineInstance.DefaultFont, score, drawPosition,
				Color.LightGoldenrodYellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			var level = string.Concat("Level ", _enemies.Level);
			textSize = Game.EngineInstance.DefaultFont.MeasureString(level);
			drawPosition = new Vector2(Game.EngineInstance.ScreenWidth / 2f - textSize.X / 2f, 10);
			Game.EngineInstance.DefaultSpriteBatch.DrawString(
				Game.EngineInstance.DefaultFont, level, drawPosition,
				Color.LightGoldenrodYellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			//var mines = _destroyer.Mines.Aggregate("", (current, t) => string.Concat(current, " | "));
			var mines = "";
			for (var i = 0; i < 10 - _destroyer.Mines.Count; i++)
				mines += "|";
			textSize = Game.EngineInstance.DefaultFont.MeasureString(mines);
			drawPosition = new Vector2(10f, 5 + textSize.Y);
			Game.EngineInstance.DefaultSpriteBatch.DrawString(
				Game.EngineInstance.DefaultFont, mines, drawPosition,
				Color.LightGoldenrodYellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			if (!IsActive && Lives < 0)
				Game.EngineInstance.DefaultSpriteBatch.Draw(
					_textureGameOver, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
					Game.EngineInstance.Scale, SpriteEffects.None, 0f);

			if (IsActive && Lives >= 0 && _enemies.IsNextLevel())
				Game.EngineInstance.DefaultSpriteBatch.Draw(
					_textureNextLevel, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
					Game.EngineInstance.Scale, SpriteEffects.None, 0f);

			Game.EngineInstance.DefaultSpriteBatch.End();
		}

		private void DrawWaves(GameTime gameTime, int firstIndex, int lastIndex)
		{
			for (var i = firstIndex; i <= lastIndex; i++)
				_waves[i].Draw(gameTime);
		}

		private void GameOver()
		{
			IsActive = false;
		}
	}
}
