using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class Collectable : Entity
    {
        public static Collectable instance;

        public static readonly float MOVE_SPEED = 760f;
        public int levelX, levelY, tileX, tileY;
        
        public Collectable(int x,int y, int levelX, int levelY, int tileX, int tileY)
            : base(new Hitbox(32, 32))
        {
            position = new Vector2(x, y);
            hitbox.Update(this);
            this.setTexture("collectable");
            instance = this;
            this.levelX = levelX;
            this.levelY = levelY;
            this.tileX = tileX;
            this.tileY = tileY;
        }

        public override void Update(GameTime gameTime)
        {
            if (Hero.instance.hitbox.intersects(hitbox))
            {
                Game1.levelManager.collectablesToRemove.Add(this);
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
