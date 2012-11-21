using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{

    public class Bullet : Entity
    {
        public bool shoot;
        public int damage;
        public bool phasing;
        public Vector2 movement;
        public Vector2 lastHeroPosition;
        public Vector2 heroPosition;
        public bool dying = false;
        public Emitter trailEmitter;
        public Emitter explosionEmitter;
        public static readonly float DISTANCE_LIMIT_NORMAL = 650f;
        public static readonly float DISTANCE_LIMIT_CHARGE = 400f;
        public float distanceLimit;
        public float distance;
        public string textureName;

        public static readonly float MOVE_SPEED = 900f;

        public Bullet(string textureName, PrebuiltEmitter trailEmitterType, Color emitterColor, Direction dir, float distanceLimit, int damage, bool phasing = false)
            : base(new Hitbox(0, 0))
        {
            position = new Vector2(0, 0);
            this.textureName = textureName;
            this.setTexture(textureName);
            this.damage = damage;
            this.distanceLimit = distanceLimit;
            this.phasing = phasing;
            heroPosition = new Vector2(Hero.instance.position.X, Hero.instance.position.Y);
            lastHeroPosition = heroPosition;
            float emitterAngle = 0;
            switch (dir)
            {
                case Direction.Up:
                    velocity = new Vector2(0, -MOVE_SPEED);
                    emitterAngle = (float)Math.PI / 2;
                    rotation = 0;
                    break;
                case Direction.Down:
                    velocity = new Vector2(0, MOVE_SPEED);
                    emitterAngle = (float)Math.PI * 3 / 2;
                    rotation = (float)Math.PI;
                    break;
                case Direction.Left:
                    velocity = new Vector2(-MOVE_SPEED, 0);
                    emitterAngle = 0;
                    rotation = (float)Math.PI * 3 / 2;
                    break;
                case Direction.Right:
                    velocity = new Vector2(MOVE_SPEED, 0);
                    emitterAngle = (float)Math.PI;
                    rotation = (float)Math.PI / 2;
                    break;
            }
            trailEmitter = Emitter.getPrebuiltEmitter(trailEmitterType);
            //trailEmitter.targetAngle = emitterAngle;
            trailEmitter.angle = emitterAngle;
            trailEmitter.startColor = emitterColor;
            emitterColor.A = 0;
            trailEmitter.endColor = emitterColor;

            explosionEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.BulletHitExplosion);
            emitterColor = new Color(emitterColor.R / 2, emitterColor.G / 2, emitterColor.B / 2, 255);
            explosionEmitter.startColor = emitterColor;
            emitterColor.A = 0;
            explosionEmitter.endColor = emitterColor;
            explosionEmitter.active = false;
        }
        public Bullet(string textureName, Hitbox hitbox, int damage, Vector2 position, Vector2 velocity, float rotation, float scale, bool phasing, int texFrame, History.EmitterHistory trail, History.EmitterHistory explosion, bool dying)
        {
            this.textureName = textureName;
            setTexture(textureName);
            this.hitbox = hitbox;
            this.damage = damage;
            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
            this.scale = scale;
            this.phasing = phasing;
            setTextureFrame(texFrame);
            this.trailEmitter = trail.emitter;
            this.trailEmitter.particlesEmitted = trail.emitCount;
            this.trailEmitter.active = trail.emitterActive;
            this.explosionEmitter = explosion.emitter;
            this.explosionEmitter.particlesEmitted = explosion.emitCount;
            this.explosionEmitter.active = explosion.emitterActive;
            this.dying = dying;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            trailEmitter.position = position;
            trailEmitter.Update(gameTime);
            if (dying)
            {
                trailEmitter.active = false;
                explosionEmitter.Update(gameTime);
                if (explosionEmitter.isFinished() && trailEmitter.isFinished())
                {
                    Game1.levelManager.bulletsToRemove.Add(this);
                }
            }
            else
            {
                movement = velocity * seconds;
                float nextX = this.position.X + movement.X;
                float nextY = this.position.Y + movement.Y;
                distance += movement.Length();

                bool collided = false;
                if ((Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) ||
                            Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) ||
                            Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) ||
                            Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y))))
                {
                    collideWith(null);
                    collided = true;
                }
                if (!collided || phasing)
                {
                    position = position + movement;
                }
                if (distance > distanceLimit)
                    dying = true;

                explosionEmitter.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public void collideWith(Enemy e)
        {
            if (e != null)
            {
                if (phasing)
                    e.bulletsAlreadyHit.Add(this);
                if (dying)
                    return;
                e.hp -= damage;
                if (e.hp <= 0)
                {
                    e.die();
                }
            }
            if (phasing && e == null)
            {
            }
            else
            {
                explosionEmitter.position = position;
                explosionEmitter.particlesEmitted = 0;
                explosionEmitter.active = true;
                trailEmitter.active = false;
            }
            if (!phasing)
            {
                dying = true;
            }
        }
        

        public override void Draw(SpriteBatch spriteBatch)
        {
            trailEmitter.Draw(spriteBatch);
            explosionEmitter.Draw(spriteBatch);
            if (!dying)
                base.Draw(spriteBatch);
        }
    }
}