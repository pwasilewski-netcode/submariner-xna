using Submariner.Vessels;

namespace Submariner.Agents
{
	public abstract class AgentFactory
	{
		public abstract Agent CreateAgent(Submarine submarine);
	}
}
