using Microsoft.Xna.Framework;

namespace Submariner.Environment
{
	public class Seabed : SpriteScroll
	{
		public Seabed(float speed)
			: base(speed, "Environment/seabed", Color.DarkGray)
		{
		}
	}
}
