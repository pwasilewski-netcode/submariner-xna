using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Submariner.Screens;
using System.Linq;

namespace Submariner
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
    	private Screen _currentScreen;

    	private int _fps;
    	private int _fpsCount;
    	private double _elapsedTime;

    	private readonly Screen[] _screens = new Screen[]
		{
			new MainScreen(),
			new GameScreen()
		};

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

			Array.ForEach(_screens, s => s.ChangeScreen += ChangeScreen);
        }

		private void ChangeScreen(string nextScreen)
		{
			lock (GetType())
			{
				var next = _screens.FirstOrDefault(s => s.GetType().Name.StartsWith(nextScreen));
				if (next != null)
				{
					_currentScreen.IsActive = false;
					_currentScreen = next;
					_currentScreen.IsActive = true;
					//Exit();
					//return;
				}
			}
		}

		public static Engine EngineInstance { get; private set; }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
			_currentScreen = _screens[0];
        	_currentScreen.IsActive = true;
        	base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
			EngineInstance = new Engine(_graphics, GraphicsDevice, new Vector2(640, 360), false);
			//EngineInstance = new Engine(_graphics, GraphicsDevice, new Vector2(1920, 1080));
			//EngineInstance = new Engine(_graphics, GraphicsDevice, new Vector2(1280, 720));
			//EngineInstance = new Engine(_graphics, GraphicsDevice, new Vector2(1366, 768));
			EngineInstance.LoadContent(Content);

        	Array.ForEach(_screens, s => s.LoadContent(Content));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
			Array.ForEach(_screens, s => s.UnloadContent());
			EngineInstance.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
			if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.LeftAlt) && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.F4))
                Exit();

        	_elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
			if (_elapsedTime > 1)
			{
				_elapsedTime = 0;
				_fps = _fpsCount;
				_fpsCount = 0;
			}

			_currentScreen.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
			GraphicsDevice.Clear(Color.Black);
        	_fpsCount++;
			_currentScreen.Draw(gameTime);
			//EngineInstance.DefaultSpriteBatch.Begin();
			//EngineInstance.DrawString(string.Concat("fps: ", _fps.ToString()), Vector2.Zero, Color.Yellow);
			//EngineInstance.DefaultSpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
