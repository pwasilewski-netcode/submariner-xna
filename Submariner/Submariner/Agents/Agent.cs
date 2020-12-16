using Microsoft.Xna.Framework;
using Submariner.Vessels;

namespace Submariner.Agents
{
	public abstract class Agent
	{
		protected Submarine Submarine;

		protected Agent(Submarine submarine)
		{
			Submarine = submarine;
		}
		
		public abstract void Update(GameTime gameTime);
	}
}
