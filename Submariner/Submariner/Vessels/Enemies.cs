using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Submariner.Agents;
using Submariner.Environment;

namespace Submariner.Vessels
{
	public class Enemies : IDisposable
	{
		private readonly ContentManager _content;
		private readonly Destroyer _destroyer;
		private readonly Random _random = new Random();
		private readonly float _top;
		private readonly Color[] _colors = { Color.Brown, Color.White, Color.Wheat, Color.Yellow, Color.Gold };
		private const int SizeStep = 25;
		private float _nextLevelTimeout;
		private readonly Wave _wave;

		public Enemies(ContentManager content, Destroyer destroyer, float top, Wave wave, int level = 3)
		{
			Submarines = new List<Submarine>();
			Mines = new List<SubmarineMine>();
			IsActive = true;
			_content = content;
			_top = top;
			_wave = wave;
			Level = level;
			_nextLevelTimeout = 0f;
			_destroyer = destroyer;
		}

		public int SubmarinesCount
		{
			get { return Level < 3 ? 1 : 2; }
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
					Level = 1;
					//    Submarines.ForEach(s => s.UnloadContent());
					//    Submarines.Clear();
					//    Mines.ForEach(m => m.UnloadContent());
					//    Mines.Clear();
				}
			}
		}

		public List<Submarine> Submarines { get; private set; }

		public List<SubmarineMine> Mines { get; private set; }

		public int Level { get; private set; }

		private AgentFactory CreateAgentFactory()
		{
			if (Level == 1)
			    return new SimpleAgentFactory();
			if (Level == 2)
			    return new SonarAgentFactory(_destroyer);
			return new CooperateAgentFactory(_destroyer);
		}

		private void AddSubmarine()
		{
			var speed = (float)_random.NextDouble() + Level;
			if (speed < 1f)
				speed = 1f;

			if (speed > 4)
				speed = 4f;

			var top = .5f + (float)_random.NextDouble() / 2f;
			if (top < .6)
				top = .6f;
			var size = (int)(_random.NextDouble() * 8) * SizeStep;
			var toLeft = _random.NextDouble() > .5d;
			var submarine2 = Submarines.FirstOrDefault(s => s.Agent is CooperateAgent);
			if (submarine2 != null && Level >= 3)
			{
				var y = submarine2.Position.Y / Game.EngineInstance.ScreenHeight;
				var d = submarine2.GetHeight() / Game.EngineInstance.ScreenHeight;
				if (y + 2.5 * d > 1)
					y = 1;
				else if (y - 2.5 * d < .6f)
					y = .6f + d;
				else
					y += 2 * d;
				top = y;
				toLeft = !submarine2.ToLeft;
			}
			var submarine = new Submarine(this, CreateAgentFactory(), toLeft, speed, top, size, size - SizeStep, _colors[_random.Next(0, _colors.Length)]);
			submarine.LoadContent(_content);
			if (submarine2 != null)
			{
				var cooperateAgent1 = submarine.Agent as CooperateAgent;
				var cooperateAgent2 = (CooperateAgent)submarine2.Agent;
				if (cooperateAgent1 != null && cooperateAgent2 != null)
				{
					cooperateAgent1.NextScanTimeout = CooperateAgent.ScanTimeout / 2f;
					cooperateAgent2.NextScanTimeout = CooperateAgent.ScanTimeout;
					cooperateAgent1.PushScan += cooperateAgent2.ScanReceived;
					cooperateAgent2.PushScan += cooperateAgent1.ScanReceived;
				}
			}
			Submarines.Add(submarine);
		}

		public void Update(GameTime gameTime)
		{
			if (!IsActive)
			{
				Submarines.ForEach(s => s.UnloadContent());
				Submarines.Clear();
				Mines.ForEach(m => m.UnloadContent());
				Mines.Clear();
				return;
			}

			if (IsNextLevel())
			{
				_nextLevelTimeout -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
				if (_nextLevelTimeout <= 0f)
					_nextLevelTimeout = 0f;
			}

			if (!IsNextLevel() && Submarines.Count < SubmarinesCount)
				AddSubmarine();

			//foreach (var submarine in Submarines.Where(submarine => submarine.IsOutOfBoard()))
			//    submarine.ToLeft = !submarine.ToLeft;

			foreach (var submarine in Submarines)
			{
				submarine.Update(gameTime);

				if (IsNextLevel())
					continue;

				//if ((submarine.MovementVector.X < 0 && submarine.Position.X < Game.EngineInstance.ScreenWidth * .9f && submarine.Position.X > Game.EngineInstance.ScreenWidth * .1f)
				//    || (submarine.MovementVector.X >= 0 && submarine.Position.X + submarine.GetWidth() < Game.EngineInstance.ScreenWidth * .9f && submarine.Position.X > Game.EngineInstance.ScreenWidth * .1f))
				//{
				//    for (var i = 0; i < submarine.Shoots.Length; i++)
				//    {
						//if (submarine.Shoots[i] > 0f
						//    && submarine.Position.X >= Game.EngineInstance.ScreenWidth * submarine.Shoots[i]
						//    && (float)_random.NextDouble() > _mineThresold)
						//{
						//    submarine.Shoots[i] = 0f;
						//    Shoot(submarine);
						//}
				//    }
				//}
			}

			foreach (var mine in Mines)
			{
				mine.Update(gameTime);
			}
		}

		public void DrawSubmarines(GameTime gameTime)
		{
			if (!IsActive)
				return;

			foreach (var submarine in Submarines)
				submarine.Draw(gameTime);
		}

		private void DrawMines(GameTime gameTime, bool front)
		{
			if (!IsActive)
				return;

			foreach (var mine in Mines.Where(m => m.IsTop == front))
				mine.Draw(gameTime);
		}

		public void DrawMinesBack(GameTime gameTime)
		{
			DrawMines(gameTime, false);
		}

		public void DrawMinesFront(GameTime gameTime)
		{
			DrawMines(gameTime, true);
		}
		
		public void Shoot(Submarine submarine)
		{
			var mine = new SubmarineMine(submarine, _top, _wave);
			mine.LoadContent(Game.EngineInstance.Content);
			Mines.Add(mine);
		}

		public void NextLevel()
		{
			Level++;
			_nextLevelTimeout = 2000;
		}

		public bool IsNextLevel()
		{
			return _nextLevelTimeout > 0;
		}

		public void Dispose()
		{
			IsActive = false;
			Submarines.ForEach(s => s.UnloadContent());
			Submarines.Clear();
		}
	}
}
