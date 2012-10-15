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
    public class Entity
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
        public bool active;

        public Hitbox hitbox;
        protected AnimatedTexture texture;
        public float scale = 1;
        public float alpha = 1;
        public float rotation = 0;
        public Direction direction;
        public float layer = 0.5f;
        public static readonly Vector2 INITIAL_POSITION = new Vector2(300, 0);

        public Entity()
        {
            active = true;
            position = INITIAL_POSITION;
            hitbox = new Hitbox(new Vector2(0, 0), 0, 0);
        }

        public Entity(Hitbox hitbox)
        {
            active = true;
            position = INITIAL_POSITION;
            this.hitbox = hitbox;
        }

        public void setPosition(Vector2 v)
        {
            position = v;
        }

        public int getTextureFrame()
        {
            return texture.frame;
        }

        public virtual Texture2D getTexture()
        {
            return texture.getTexture();
        }

        public Vector2 getTop()
        {
            return position + new Vector2(0, -hitbox.height / 2f) + hitbox.offset;
        }

        public Vector2 getBottom()
        {
            return position + new Vector2(0, hitbox.height / 2f) + hitbox.offset;
        }

        public Vector2 getLeft()
        {
            return position + new Vector2(-hitbox.width / 2f, 0) + hitbox.offset;
        }

        public Vector2 getRight()
        {
            return position + new Vector2(hitbox.width / 2f, 0) + hitbox.offset;
        }

        public void setTop(float Y)
        {
            position = new Vector2(position.X, Y + getTexture().Height / 2f);
        }

        public void setBottom(float Y)
        {
            position = new Vector2(position.X, Y + getTexture().Height / 2f);
        }

        public void setLeft(float X)
        {
            position = new Vector2(X + getTexture().Height / 2f, position.Y);
        }

        public void setRight(float X)
        {
            position = new Vector2(X - texture.getTexture().Height / 2f, position.Y);
        }

        public float getTransformedScale()
        {
            return getBottom().Y / Game1.screenSize.Y;
        }

        public void setTextureFrame(int frame)
        {
            if (!texture.animated)
                return;
            if (frame < 1 || frame > texture.framemax)
                throw new ArgumentOutOfRangeException("frame", "Frame needs to be between 1 and framemax");
            texture.frame = frame;
        }

        public void setTexture(string tex)
        {
            texture = new AnimatedTexture(tex);
        }

        public void setTexture(AnimatedTexture tex)
        {
            if (texture != null)
                foreach (Hitbox hitbox in texture.onFrameHitboxes)
                    if (hitbox != null)
                    {
                        hitbox.active = false;
                    }
            texture = tex;
            texture.Update(null, this);
            if (texture.onStart != null)
                texture.onStart();
        }

        public void offsetPosition(Vector2 off)
        {
            position += off;
        }

        public bool canMove(Vector2 movement)
        {
            return Game1.levelManager.attemptScroll(this, movement);
        }

        public virtual SpriteEffects getFlip()
        {
            return SpriteEffects.None;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!active)
                return;
            texture.Update(gameTime, this);
            hitbox.Update(this);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!active)
                return;
            Texture2D tex = getTexture();
            if (tex == null)
                return;
            spriteBatch.Draw(tex, position, null, Color.White, rotation, new Vector2(getTexture().Width / 2, getTexture().Height / 2), scale, getFlip(), layer);
        }
        public virtual void DrawDebug(SpriteBatch spriteBatch)
        {
            if (!active)
                return;
            hitbox.Draw(spriteBatch);
            texture.DrawHitbox(spriteBatch);
        }
    }
}
