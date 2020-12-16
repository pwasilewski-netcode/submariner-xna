using Submariner.Vessels;

namespace Submariner.Agents
{
	public sealed class SimpleAgentFactory : AgentFactory
	{
		public override Agent CreateAgent(Submarine submarine)
		{
			return new SimpleAgent(submarine);
		}
	}
}
