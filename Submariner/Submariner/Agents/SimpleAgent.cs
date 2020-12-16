using Microsoft.Xna.Framework;
using Submariner.Vessels;

namespace Submariner.Agents
{
	public class SimpleAgent : Agent
	{
		public SimpleAgent(Submarine submarine)
			: base(submarine) { }

		public override void Update(GameTime gameTime)
		{
			Submarine.Shoot();

			if (Submarine.IsOutOfBoard())
				Submarine.Move(new Vector2(-1f * Submarine.MovementVector.X, 0f));
		}
	}
}
