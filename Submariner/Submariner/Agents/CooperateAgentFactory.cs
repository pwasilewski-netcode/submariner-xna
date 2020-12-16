using Submariner.Vessels;

namespace Submariner.Agents
{
	public sealed class CooperateAgentFactory : AgentFactory
	{
		private readonly Destroyer _destroyer;

		public CooperateAgentFactory(Destroyer destroyer)
		{
			_destroyer = destroyer;
		}

		public override Agent CreateAgent(Submarine submarine)
		{
			return new CooperateAgent(submarine, _destroyer);
		}
	}
}
