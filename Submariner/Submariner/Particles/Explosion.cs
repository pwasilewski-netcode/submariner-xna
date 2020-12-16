using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Submariner.Particles
{
    public class Explosion : ISprite
    {
        private struct Particle
        {
        	public float ElapsedTime;
            public float LifeTime;
            public Vector2 OrginalPosition;
            public Vector2 Accelaration;
            public Vector2 Direction;
            public Vector2 Position;
            public float Scale;
			public float ScaleFactor;
        	public float EpicenterSize;
            public Color Color;
        }

        private SoundEffect _explosionSound;
        private readonly List<Particle> _particles = new List<Particle>();
		private readonly Random _random = new Random();
    	private SpriteBatch _spriteBatch;

		public void Create(Vector2 position, int particleCount, float epicentreSize, float bangSize, float scaleFactor, float lifeTime)
		{
			Position = position;
			IsActive = true;
			_particles.Clear();
			for (var i = 0; i < particleCount; i++)
			{
				var particle = new Particle
				{
					OrginalPosition = position,
					Position = position,
					LifeTime = lifeTime,
					EpicenterSize = epicentreSize,
					ScaleFactor = scaleFactor,
					Color = Color.White
				};

				var particleDistance = (float)_random.NextDouble() * bangSize;
				var displacement = new Vector2(particleDistance, 0);
				var angle = MathHelper.ToRadians(_random.Next(360));
				displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

				//particle.Direction = displacement * 2.0f;
				//particle.Accelaration = -particle.Direction;

				particle.Direction = displacement;
				particle.Accelaration = 3.0f * particle.Direction;

				_particles.Add(particle);
			}
			_explosionSound.Play();
		}

		public bool Exploding
		{
			get { return _particles.Count > 0; }
		}

    	public bool IsActive { get; set; }

    	public Texture2D Texture { get; private set; }

    	public Vector2 Position { get; private set; }

    	public void LoadContent(ContentManager content)
    	{
    		_spriteBatch = Game.EngineInstance.CreateSpriteBatch();
        	_explosionSound = content.Load<SoundEffect>("Particles/explosion_sound");
			Texture = content.Load<Texture2D>("Particles/explosion");
        }

        public void UnloadContent()
        {
			//_explosionSound.Dispose();
			//Texture.Dispose();
			_spriteBatch.Dispose();
        }

        public void Update(GameTime gameTime)
        {
			if (!IsActive)
				return;

        	var elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			for (var i = _particles.Count - 1; i >= 0; i--)
			{
				var particle = _particles[i];
				particle.ElapsedTime += elapsed;
				if (particle.ElapsedTime > particle.LifeTime)
				{
					_particles.RemoveAt(i);
					continue;
				}

				var timeFactor = particle.ElapsedTime / particle.LifeTime;
				particle.Position = 0.5f * particle.Accelaration * timeFactor * timeFactor + particle.Direction * timeFactor + particle.OrginalPosition;

				var inversedTime = 1.0f - timeFactor;
				particle.Color = new Color(new Vector4(inversedTime, inversedTime, inversedTime, inversedTime));

				var positionFromCenter = particle.Position - particle.OrginalPosition;
				particle.Scale = (particle.EpicenterSize + positionFromCenter.Length()) / particle.ScaleFactor;

				_particles[i] = particle;
			}
        }

        public void Draw(GameTime gameTime)
        {
			if (!IsActive)
				return;

			DrawExplosion(BlendState.NonPremultiplied);
			DrawExplosion(BlendState.AlphaBlend);
			DrawExplosion(BlendState.Additive);
        }

		private void DrawExplosion(BlendState blendState)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, blendState);
			for (var i = 0; i < _particles.Count; i++)
			{
				var particle = _particles[i];
				_spriteBatch.Draw(
					Texture,
					particle.Position,
					null,
					particle.Color,
					i,
					new Vector2(Texture.Width / 2f, Texture.Height / 2f),
					particle.Scale,
					SpriteEffects.None,
					1);
			}
			_spriteBatch.End();
		}
    }
}
