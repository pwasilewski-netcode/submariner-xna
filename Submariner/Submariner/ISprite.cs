using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Submariner
{
    public interface ISprite
    {
		bool IsActive { get; set; }
    	Texture2D Texture { get; }
    	Vector2 Position { get; }
        void LoadContent(ContentManager content);
        void UnloadContent();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
