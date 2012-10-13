using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{

    public class Bullet : Entity
    {
        public static Bullet instance;
        public bool shoot;
        public bool lockOrientation;
        public Vector2 movement;
        public int orientation;
        public int shootType;
        public float speedAdjust;
        public bool sideDirection;
        public Vector2 lastHeroPosition;
        public Vector2 heroPosition;

        public static readonly float MOVE_SPEED = 1100f;

        public Bullet(Direction dir)
            : base(new Hitbox(16, 16))
        {
            position = new Vector2(0, 0);
            this.setTexture("bullet");
            instance = this;
            orientation = 0;
            lockOrientation = false;
            sideDirection = false;
            heroPosition = new Vector2(Hero.instance.position.X, Hero.instance.position.Y);
            lastHeroPosition = heroPosition;
            speedAdjust = MOVE_SPEED;
            switch (dir)
            {
                case Direction.Up:
                    velocity = new Vector2(0, -MOVE_SPEED);
                    break;
                case Direction.Down:
                    velocity = new Vector2(0, MOVE_SPEED);
                    break;
                case Direction.Left:
                    velocity = new Vector2(-MOVE_SPEED, 0);
                    break;
                case Direction.Right:
                    velocity = new Vector2(MOVE_SPEED, 0);
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            movement = velocity * seconds;
            float nextX = this.position.X + movement.X;
            float nextY = this.position.Y + movement.Y;

            if (((Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) ||
                        Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) ||
                        Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) ||
                        Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y))) ||
                        this.hitbox.intersects(Enemy.instance.hitbox)) &&
                        Hero.instance.powerUp3 != 2)
            {
                Game1.levelManager.bulletsToRemove.Add(this);
            }
            else
            {
                position = position + movement;
            }

            lastHeroPosition = heroPosition;
            base.Update(gameTime);
        }
        

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}