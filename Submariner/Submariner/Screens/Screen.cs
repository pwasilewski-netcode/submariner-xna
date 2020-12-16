using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Submariner.Screens
{
	public abstract class Screen
	{
		public abstract bool IsActive { get; set; }
		public abstract void LoadContent(ContentManager content);
		public abstract void UnloadContent();
		public abstract void Update(GameTime gameTime);
		public abstract void Draw(GameTime gameTime);

		public event ChangeScreenHandler ChangeScreen;

		protected virtual void RaiseChangeScreen(string nextScreen)
		{
			if (ChangeScreen == null)
				return;

			ChangeScreen(nextScreen);
		}
	}

	public delegate void ChangeScreenHandler(string nextScreen);
}
