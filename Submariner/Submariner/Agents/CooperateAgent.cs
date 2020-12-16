using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Submariner.Vessels;

namespace Submariner.Agents
{
	public delegate void ScanHandler(Vector2 destroyerPosition, List<DestroyerMine> mines);

	public class CooperateAgent : Agent
	{
		private readonly Destroyer _destroyer;
		private List<DestroyerMine> _mines;
		public const float ScanTimeout = 2000;
		private Vector2 _atractorVector;

		public void ScanReceived(Vector2 destroyerPosition, List<DestroyerMine> mines)
		{
			_mines = mines.ToList();
			_atractorVector = destroyerPosition;
			if (_atractorVector == _destroyer.Position)
				_atractorVector.X += _destroyer.GetWidth();

			React();
		}

		public float NextScanTimeout { get; set; }

		public event ScanHandler PushScan;

		public CooperateAgent(Submarine submarine, Destroyer destroyer)
			: base(submarine)
		{
			_destroyer = destroyer;
			_mines = new List<DestroyerMine>();
			NextScanTimeout = ScanTimeout;
		}

		private void Scan()
		{
			if (NextScanTimeout <= 0f)
			{
				_atractorVector = _destroyer.Position;
				NextScanTimeout = ScanTimeout;
				var distance = Vector2.DistanceSquared(Submarine.Position, _atractorVector) / Math.Abs(Submarine.Position.Y - _atractorVector.Y);
				if (distance > 1)
					distance = 1f;
				Submarine.PlaySonarSound(distance);

				_mines = _destroyer.Mines.Where(m => m.Position.Y < Submarine.Position.Y - Submarine.GetHeight()).OrderBy(m => m.Position.X).ToList();
				React();


				if (PushScan != null)
					PushScan(_atractorVector, _mines);
				
				//if (same)
				//    _atractorVector.X -= _destroyer.GetWidth();
			}
		}

		private void Shoot()
		{
			var submarineCenter = Submarine.Position.X + Submarine.GetWidth() / 2f;
			if (submarineCenter >= _destroyer.Position.X - 20f && submarineCenter <= _destroyer.Position.X + _destroyer.GetWidth() + 20f)
				Submarine.Shoot();
			var mine = _mines.FirstOrDefault(m => submarineCenter >= m.Position.X - 20f && submarineCenter <= m.Position.X + 20f);
			if (mine != null)
				Submarine.Shoot();
		}

		public override void Update(GameTime gameTime)
		{
			NextScanTimeout -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			Scan();

			var vector = Vector2.Zero;
			if (_atractorVector.X > Submarine.Position.X)
				vector.X = 1;
			else
				vector.X = -1;

			Submarine.Move(vector);

			Shoot();
		}

		private void React()
		{
			var atractorPositions = _mines.Select(m => new
			{
				m.Position,
				TimeToBlast = Submarine.GetDestroyerMineTimeToBlast(m),
				Time = Submarine.GetTimeToDestination(m.Position, Submarine.MovementVector)
			}).Select(ap => new { ap.Position, ap.Time, ap.TimeToBlast, Delta = ap.TimeToBlast - ap.Time })
			.Where(m => m.TimeToBlast <= 5 && m.TimeToBlast > -1f).ToList();
			var width = Submarine.GetWidth() + 10f;
			var newAtractorPosition = Submarine.Position;
			if (_atractorVector.X > Submarine.Position.X)
			{
				var submarineIndex = -1;
				var submarineEndPosition = Submarine.Position;
				submarineEndPosition.X += width;
				var submarineDelta = Submarine.GetTimeToDestination(submarineEndPosition, new Vector2(1, 0));
				while (submarineIndex < atractorPositions.Count)
				{
					if (submarineIndex == -1)
						submarineIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X).Count();
					else
						submarineIndex++;
					if (submarineIndex < atractorPositions.Count/* && atractorPositions[submarineIndex].Position.X < _atractorVector.X + _destroyer.GetWidth()*/)
					{
						var ap1 = atractorPositions[submarineIndex];
						if (ap1.Delta > 1f || ap1.Delta + submarineDelta < -1f)
						{
							var index = submarineIndex + atractorPositions.Skip(submarineIndex + 1).TakeWhile(ap => ap.Position.X < ap1.Position.X).Count();
							if (index == submarineIndex || index >= atractorPositions.Count || Math.Abs(ap1.Position.X - atractorPositions[index].Position.X) >= width)
								newAtractorPosition = new Vector2(ap1.Position.X + 5f, _atractorVector.Y);
						}
						else
						{
							break;
						}
					}
					else
					{
						newAtractorPosition = _atractorVector;
					}
				}
			}
			else
			{
				var submarineEndPosition = Submarine.Position;
				submarineEndPosition.X -= width;
				var submarineDelta = Submarine.GetTimeToDestination(submarineEndPosition, new Vector2(-1, 0));
				var submarineIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X + width).Count();
				while (submarineIndex > -1)
				{
					if (submarineIndex == atractorPositions.Count)
						submarineIndex -= 1;
					else
						submarineIndex--;
					if (submarineIndex > -1)
					{
						var ap1 = atractorPositions[submarineIndex];
						if (ap1.Delta - submarineDelta > 1f || ap1.Delta < -1f)
						{
							var index = atractorPositions.TakeWhile(ap => ap.Position.X < ap1.Position.X).Count();
							if (index == submarineIndex || index >= atractorPositions.Count || Math.Abs(ap1.Position.X - atractorPositions[index].Position.X) >= width)
								newAtractorPosition = new Vector2(ap1.Position.X + 5f, _atractorVector.Y);
						}
						else
						{
							break;
						}
					}
					else
					{
						newAtractorPosition = _atractorVector;
					}
				}
			}
			_atractorVector = newAtractorPosition;
		}
	}
}
