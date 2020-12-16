using Submariner.Vessels;

namespace Submariner.Agents
{
	public sealed class SonarAgentFactory : AgentFactory
	{
		private readonly Destroyer _destroyer;
		
		public SonarAgentFactory(Destroyer destroyer)
		{
			_destroyer = destroyer;
		}

		public override Agent CreateAgent(Submarine submarine)
		{
			return new SonarAgent(submarine, _destroyer);
		}
	}
}
