using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Submariner.Vessels;
using System.Linq;

namespace Submariner.Agents
{
	public class SonarAgent : Agent
	{
		private readonly Destroyer _destroyer;
		private List<DestroyerMine> _mines;
		private const float ScanTimeout = 2000;
		private float _nextScanTimeout;
		private Vector2 _atractorVector;

		public SonarAgent(Submarine submarine, Destroyer destroyer)
			: base(submarine)
		{
			_destroyer = destroyer;
			_mines = new List<DestroyerMine>();
		}

		public override void Update(GameTime gameTime)
		{
			_nextScanTimeout -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (_nextScanTimeout <= 0f)
			{
				_atractorVector = _destroyer.Position;
				_nextScanTimeout = ScanTimeout;
				var distance = Vector2.DistanceSquared(Submarine.Position, _atractorVector) / Math.Abs(Submarine.Position.Y - _atractorVector.Y);
				if (distance > 1)
					distance = 1f;
				Submarine.PlaySonarSound(distance);

				_mines = _destroyer.Mines.Where(m => m.Position.Y < Submarine.Position.Y - Submarine.GetHeight()).OrderBy(m => m.Position.X).ToList();
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
								//if (newAtractorPosition == Submarine.Position)
								//    newAtractorPosition = new Vector2(ap1.Position.X - width - 20f, _atractorVector.Y);
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
								//if (newAtractorPosition == Submarine.Position)
								//    newAtractorPosition = new Vector2(ap1.Position.X - width - 20f, _atractorVector.Y);
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

				//_atractorVector = OmitMines();
				//var vectorToOmitMines = OmitMines();
				//if (_atractorVector == vectorToOmitMines)
				//    Submarine.UseTurbo();
				//_atractorVector = vectorToOmitMines;
			}
			var vector = Vector2.Zero;
			if (_atractorVector.X > Submarine.Position.X)
				vector.X = 1;
			//else if (_atractorVector.X < Submarine.Position.X)
			else
				vector.X = -1;

			Submarine.Move(vector);

			var submarineCenter = Submarine.Position.X + Submarine.GetWidth() / 2f;
			if (submarineCenter >= _destroyer.Position.X - 20f && submarineCenter <= _destroyer.Position.X + _destroyer.GetWidth() + 20f)
				Submarine.Shoot();
			var mine = _mines.FirstOrDefault(m => submarineCenter >= m.Position.X - 20f && submarineCenter <= m.Position.X + 20f);
			if (mine != null)
				Submarine.Shoot();
		}

		//private Vector2 OmitMines()
		//{
		//    if (_mines.Count == 0 || Submarine.MovementVector == Vector2.Zero)
		//        return _atractorVector;

		//    var atractorPositions = _mines.Select(m => new
		//    {
		//        m.Position,
		//        TimeToBlast = Submarine.GetDestroyerMineTimeToBlast(m),
		//        Time = Submarine.GetTimeToDestination(m.Position, Submarine.MovementVector)
		//    }).Where(m => m.TimeToBlast <= 5).ToList();
		//    //if (atractorPositions.Count > 0)
		//    //{
		//    //    var first = atractorPositions.First().Position;
		//    //    first.X -= Submarine.GetWidth() + 10f;
		//    //    atractorPositions.Insert(0, new { Position = first, TimeToBlast = 0f, Time = Submarine.GetTimeToDestination(first, Submarine.MovementVector) });
		//    //    var last = atractorPositions.Last().Position;
		//    //    first.X += 10f;
		//    //    atractorPositions.Insert(atractorPositions.Count, new { Position = last, TimeToBlast = 0f, Time = Submarine.GetTimeToDestination(last, Submarine.MovementVector) });
		//    //}
		//    //atractorPositions.Add(new { Submarine.Position, TimeToBlast = 0f, Time = 1f });
		//    //atractorPositions.Add(new { Position = _atractorVector, TimeToBlast = 0f, Time = Submarine.GetTimeToDestination(_atractorVector, Submarine.MovementVector) });
		//    //atractorPositions = atractorPositions.OrderBy(ap => ap.Position.X).ToList();

		//    //var submarineEndIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X + Submarine.GetWidth()).Count();
		//    var width = Submarine.GetWidth() + 10f;
		//    //var submarineEndPosition = Submarine.Position;
		//    //submarineEndPosition.X += width;
		//    //var submarineDelta = Submarine.GetTimeToDestination(submarineEndPosition, new Vector2(1, 0));
		//    var submarineIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X).Count();
		//    var positions = atractorPositions.Select(p => new { p.Position, Delta = p.TimeToBlast - p.Time }).ToList();
		//    positions.Insert(submarineIndex, );
		//    var index = -1;
		//    const float threadshold = 1f;
		//    //for (var i = submarineIndex - 1; i > -1; i--)
		//    //{
		//    //    if ((positions[i].Delta > threadshold && positions[i].Delta - submarineDelta > threadshold)
		//    //        || (positions[i].Delta < -threadshold && positions[i].Delta - submarineDelta < -threadshold))
		//    //        continue;
		//    //    if (i + 1 < positions.Count && positions[i].Position.X >= Submarine.Position.X && positions[i].Position.X <= Submarine.Position.X + width)
		//    //        break;
		//    //    if (i + 1 >= positions.Count && positions[i].Position.X > _atractorVector.X)
		//    //        index = i;
		//    //    else
		//    //        index = i + 1;

		//    //    break;
		//    //}
		//    var position = _atractorVector;
		//    //if (index > -1)
		//    //{
		//    //    if (index >= positions.Count)
		//    //        position = Submarine.Position;
		//    //    else
		//    //    {
		//    //        position = positions[index].Position;
		//    //        position.X += 10f;
		//    //    }
		//    //    return position;
		//    //}

		//    if (index == -1)
		//    {
		//        submarineIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X).Count();
		//        for (var i = submarineIndex; i < positions.Count; i++)
		//        {
		//            if ((positions[i].Delta > threadshold && positions[i].Delta + submarineDelta > threadshold)
		//                || (positions[i].Delta < -threadshold && positions[i].Delta + submarineDelta < -threadshold))
		//                continue;
		//            if (i - 1 < 0 && positions[i].Position.X >= Submarine.Position.X && positions[i].Position.X <= Submarine.Position.X + width)
		//                break;
		//            //if (i - 1 > 0 && (positions[i].Position.X - positions[i - 1].Position.X <= width))
		//            //    continue;
		//            if (i - 1 >= 0 && positions[i].Position.X < _atractorVector.X)
		//                index = i;
		//            else
		//                index = i - 1;

		//            break;
		//        }
		//    }
		//    if (index > -1)
		//    {
		//        if (index == 0)
		//        {
		//            position = positions[index].Position;
		//            position.X -= width;
		//        }
		//        else
		//        {
		//            position = positions[index].Position;
		//            position.X += width;
		//        }
		//    }
		//    return position;

			//var position = _atractorVector;
			//if (index > -1)
			//{
			//    if (index >= positions.Count)
			//        position = Submarine.Position;
			//    else
			//    {
			//        position = positions[index].Position;
			//        position.X += 10f;
			//    }
			//}
			//return position;
		//}

		//private Vector2 GoRight()
		//{
		//    var atractorPositions = _mines.Select(m => new
		//    {
		//        m.Position,
		//        TimeToBlast = Submarine.GetDestroyerMineTimeToBlast(m),
		//        Time = Submarine.GetTimeToDestination(m.Position, Submarine.MovementVector)
		//    }).Where(m => m.TimeToBlast <= 5).ToList();

		//    var submarineIndex = atractorPositions.TakeWhile(ap => ap.Position.X < Submarine.Position.X).Count();
		//    if (submarineIndex == atractorPositions.Count)
		//        return Submarine.Position;

		//    const float threadshold = 1f;
		//    atractorPositions.Insert(submarineIndex, new { Submarine.Position, TimeToBlast = threadshold + .1f, Time = 0f });
		//    var submarineEndPosition = Submarine.Position.X + Submarine.GetWidth() + 10f;
		//    var positions = atractorPositions.Select(p => new { p.Position, Delta = p.TimeToBlast - p.Time }).ToList();
		//    var index = -1;
		//    for (var i = submarineIndex; i < positions.Count; i++)
		//    {
		//        if (positions[i].Delta > threadshold || positions[i].Delta < -threadshold)
		//        {
		//            var jMax = positions.TakeWhile(ap => ap.Position.X < submarineEndPosition).Count();
		//            for (var j = i + 1; j <= jMax; j++)
		//            {
		//                var t = atractorPositions[j].Time - atractorPositions[i].Time;
		//            }
		//        }
		//        break;
		//    }
		//}
	}
}
