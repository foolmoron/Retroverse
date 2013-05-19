using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public abstract class Screen : Controllable, IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; protected set; }
        public bool DrawPreviousScreen { get; protected set; }
        public bool Initialized { get; protected set; }

        public abstract void Initialize(GraphicsDevice graphicsDevice);
        public abstract void OnScreenSizeChanged();
        public abstract void Update(GameTime gameTime);        
        public abstract void PreDraw(GameTime gameTime); //Draws any extra effects or runtime textures.  DOES NOT DRAW TO BACK BUFFER.
        public abstract void Draw(GameTime gameTime);

        public abstract void Dispose();
    }
}
