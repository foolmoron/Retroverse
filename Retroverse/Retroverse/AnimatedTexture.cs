using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Retroverse
{
    public class AnimatedTexture
    {
        string baseTex;

        public bool animated = true;
        public int frame;
        public int framemax;

        int timeStep;
        int curTime;

        public Hitbox[] onFrameHitboxes;

        public Action onStart;
        public Action onFinish;
        private Action[] onFrameActions; // use setOnFrameActions

        public AnimatedTexture(string _base)
        {
            baseTex = _base;
            animated = false;
            onFrameHitboxes = new Hitbox[1];
            onFrameHitboxes[0] = null;
            frame = 1;
        }

        public AnimatedTexture(string _base, int numframes, int _timeStep)
        {
            baseTex = _base;
            animated = true;
            framemax = numframes;
            onFrameActions = new Action[numframes];
            onFrameHitboxes = new Hitbox[numframes];
            for (int i = 0; i < onFrameHitboxes.Length; i++)
            {
                onFrameHitboxes[i] = null;
            }
            frame = 1;
            timeStep = _timeStep;
        }

        public Hitbox getHitbox()
        {
            return onFrameHitboxes[frame - 1];
        }

        public string get()
        {
            if (animated) return baseTex + "" + frame;
            else return baseTex;
        }

        public Texture2D getTexture()
        {
            return TextureManager.Get(get());
        }

        public void setOnFrameAction(int frame, Action action)
        {
            if (frame < 1 || frame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frame needs to be between 1 and framemax");
            onFrameActions[frame - 1] = action;
        }

        public void setOnFrameHitbox(int frame, Hitbox hitbox)
        {
            if (frame < 1 || frame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frame needs to be between 1 and framemax");
            if (hitbox == null)
                onFrameHitboxes[frame - 1] = new Hitbox(0, 0);
            else
                onFrameHitboxes[frame - 1] = hitbox;
        }

        public void setOnFrameRangeHitbox(int startFrame, int endFrame, Hitbox hitbox)
        {
            if (startFrame < 1 || startFrame > framemax || endFrame < 1 || endFrame > framemax)
                throw new ArgumentOutOfRangeException("frame", "Frames need to be between 1 and framemax");
            if (endFrame < startFrame)
                throw new ArgumentOutOfRangeException("frame", "End frame cannot be less than start frame");
            for (int i = startFrame; i <= endFrame; i++)
            {
                if (hitbox == null)
                    onFrameHitboxes[i - 1] = new Hitbox(0, 0);
                else
                    onFrameHitboxes[i - 1] = hitbox;
            }
        }

        public void increment()
        {
            if (frame >= framemax)
            {
                frame = 0;
                if (onFinish != null) onFinish();
            }
            if (onFrameActions != null)
                if (onFrameActions[frame] != null)
                    onFrameActions[frame]();
            frame++;
        }

        public void Update(GameTime gameTime)
        {
            curTime += (gameTime != null) ? gameTime.ElapsedGameTime.Milliseconds : 0;
            if (curTime > timeStep)
            {
                increment();
                curTime = 0;
            }
        }

        public void Update(GameTime gameTime, Entity owner)
        {
            Update(gameTime);
            if (frame > 0 && getHitbox() != null)
            {
                getHitbox().active = true;
                getHitbox().Update(owner);
            }
        }

        public void DrawHitbox(SpriteBatch spriteBatch)
        {
            if (frame > 0 && getHitbox() != null)
                getHitbox().Draw(spriteBatch);
        }
    }
}
