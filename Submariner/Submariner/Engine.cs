using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Submariner
{
	public partial class Game
	{
		public sealed class Engine
		{
			private const float ScaleFactor = .7f;
			private readonly GraphicsDeviceManager _graphics;
			private readonly GraphicsDevice _graphicsDevice;
			private ContentManager _content;
			private readonly bool _fullScreen;
			private SpriteBatch _spriteBatch;
			private SpriteFont _spriteFont;
			private readonly int _resolutionX;
			private readonly int _resolutionY;
			private readonly Vector2 _scale;
			private readonly Vector2 _scaleWithFactor;
			public readonly float X = 1920f;
			public readonly float Y = 1080f;

			public SpriteBatch DefaultSpriteBatch
			{
				get { return _spriteBatch; }
			}

			public SpriteFont DefaultFont
			{
				get { return _spriteFont; }
			}

			public ContentManager Content
			{
				get { return _content; }
			}


			public int ScreenWidth
			{
				get { return _graphicsDevice.Viewport.Width; }
			}

			public int ScreenHeight
			{
				get { return _graphicsDevice.Viewport.Height; }
			}

			internal Engine(GraphicsDeviceManager graphicsDeviceManager, GraphicsDevice graphicsDevice, Vector2 resolution, bool fullScreen = true)
			{
				_fullScreen = fullScreen;
				_graphics = graphicsDeviceManager;
				_graphicsDevice = graphicsDevice;
				_resolutionX = (int)resolution.X;
				_resolutionY = (int)resolution.Y;
				_scale = new Vector2(_resolutionX / X, _resolutionY / Y);
				_scaleWithFactor = new Vector2(_scale.X * ScaleFactor, _scale.Y * ScaleFactor);
			}

			internal void LoadContent(ContentManager contentManager)
			{
				_content = contentManager;
				_spriteBatch = new SpriteBatch(_graphicsDevice);
				_graphics.PreferredBackBufferWidth = _resolutionX;
				_graphics.PreferredBackBufferHeight = _resolutionY;
				_graphics.IsFullScreen = _fullScreen;
				_graphics.ApplyChanges();
				_spriteFont = contentManager.Load<SpriteFont>("font");
			}

			internal void UnloadContent()
			{
				_spriteBatch.Dispose();
			}

			public Vector2 Scale
			{
				get { return _scale; }
			}

			/*public void Draw(ISprite sprite)
			{
				Draw(sprite, SpriteSortMode.FrontToBack, BlendState.AlphaBlend, Color.White);
			}

			public void Draw(ISprite sprite, Color color)
			{
				Draw(sprite, SpriteSortMode.FrontToBack, BlendState.AlphaBlend, color);
			}

			public void Draw(ISprite sprite, SpriteSortMode spriteSortMode, BlendState blendState, Color color)
			{
				if (!sprite.IsActive)
					return;
				
				_spriteBatch.Draw(
					sprite.Texture, sprite.Position, null, color, 0f, Vector2.Zero,
					GetScale(),
					SpriteEffects.None, 0f);
				//_spriteBatch.Draw(sprite.Texture, new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, (int)((sprite.Texture.Width / X) * _resolutionX), (int)((sprite.Texture.Height / Y) * _resolutionY)), color);
			}*/

			public void DrawString(string text, Vector2 position, Color color)
			{
				_spriteBatch.DrawString(_spriteFont, text, position, color);
			}

			public Vector2 GetScale()
			{
				return _scaleWithFactor;
			}

			public Vector2 ScaleTexture(Texture2D texture2D)
			{
				//return new Vector2(texture2D.Width / X * _resolutionX * ScaleFactor, texture2D.Height / Y * _resolutionY * ScaleFactor);
				var scale = GetScale();
				return new Vector2(texture2D.Width * scale.X, texture2D.Height * scale.Y);
			}

			public SpriteBatch CreateSpriteBatch()
			{
				return new SpriteBatch(_graphicsDevice);
			}

			public Texture2D CreateTexture2D(int width, int height)
			{
				return new Texture2D(_graphicsDevice, width, height);
			}

			public Texture2D CreateTexture2D(int width, int height, Color color)
			{
				var texture = new Texture2D(_graphicsDevice, width, height);
				var colors = new Color[width * height];
				for (var i = 0; i < colors.Length; i++)
					colors[i] = color;
				texture.SetData(colors);
				return texture;
			}

			public void ClearGraphicsDevice(Color color)
			{
				_graphicsDevice.Clear(color);
			}

			public void Update(GameTime gameTime)
			{
			}

			public bool Collides(Color[,] color1, Vector2 position1, Color[,] color2, Vector2 position2)
			{
				var width1 = color1.GetLength(0);
				var height1 = color1.GetLength(1);
				var width2 = color2.GetLength(0);
				var height2 = color2.GetLength(1);

				var scale = GetScale();

				var p1 = new Vector2(position1.X / scale.X, position1.Y / scale.Y);
				var p2 = new Vector2(position2.X / scale.X, position2.Y / scale.Y);

				var rectangle1 = new Rectangle((int)p1.X, (int)p1.Y, width1, height1);
				var rectangle2 = new Rectangle((int)p2.X, (int)p2.Y, width2, height2);

				if (!rectangle1.Intersects(rectangle2))
					return false;

				for (var x1 = 0; x1 < width1; x1++)
				{
					for (var y1 = 0; y1 < height1; y1++)
					{
						if (color1[x1, y1].A == 0)
							continue;

						var point = new Point((int)p1.X + x1, (int)p1.Y + y1);
						if (!rectangle2.Contains(point))
							continue;

						var x2 = (int)Math.Abs(point.X - p2.X);
						var y2 = (int)Math.Abs(point.Y - p2.Y);
						if (color2[x2, y2].A > 0)
							return true;
					}
				}

				return false;
			}

			public static Color[,] TextureTo2DArray(Texture2D texture)
			{
				var colors1D = GetColorsFromTexture(texture);
				var colors2D = new Color[texture.Width, texture.Height];
				for (var x = 0; x < texture.Width; x++)
					for (var y = 0; y < texture.Height; y++)
						colors2D[x, y] = colors1D[x + y * texture.Width];

				return colors2D;
			}

			public static Color[] GetColorsFromTexture(Texture2D texture)
			{
				var colors = new Color[texture.Width * texture.Height];
				texture.GetData(colors);
				return colors;
			}
		}
	}
}
