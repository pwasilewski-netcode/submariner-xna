using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Submariner.Environment
{
	public class Sea : SpriteScroll
	{
		private SoundEffect _seaSoundEffect;
		private SoundEffectInstance _seaSoundEffectInstance;

		public Sea(float speed, Color color)
			: base(speed, "Environment/sea", color)
		{
		}

		public override bool IsActive
		{
			get { return base.IsActive; }
			set
			{
				base.IsActive = value;
				if (!base.IsActive)
					_seaSoundEffectInstance.Stop(true);
			}
		}

		public override void LoadContent(ContentManager content)
	    {
	        _seaSoundEffect = content.Load<SoundEffect>("Environment/ocean");
	        _seaSoundEffectInstance = _seaSoundEffect.CreateInstance();
			base.LoadContent(content);
	    }

	    public override void UnloadContent()
	    {
			base.UnloadContent();
	        _seaSoundEffectInstance.Dispose();
	        _seaSoundEffect.Dispose();
	    }

	    public override void Update(GameTime gameTime)
	    {
	        if (!IsActive)
	        {
	            _seaSoundEffectInstance.Stop(true);
	            return;
	        }
			
			if (_seaSoundEffectInstance.State != SoundState.Playing)
				_seaSoundEffectInstance.Play();

			base.Update(gameTime);
		}
	}
}
