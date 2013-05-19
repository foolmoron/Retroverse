using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{

    public class Bullet : Entity, IReversible
    {
        public GunPowerup gunPowerup;
        public bool shoot;
        public int damage;
        public bool phasing;
        public Vector2 movement;
        public bool dying = false;
        public Emitter trailEmitter;
        public Emitter explosionEmitter;
        public static readonly float DISTANCE_LIMIT_NORMAL = 650f;
        public static readonly float DISTANCE_LIMIT_CHARGE = 400f;
        public float distanceLimit;
        public float distance;
        public string textureName;

        public List<Enemy> enemiesAlreadyHit = new List<Enemy>();

        public static readonly float MOVE_SPEED = 900f;

        public Bullet(GunPowerup gunPowerup, string textureName, PrebuiltEmitter trailEmitterType, Color emitterColor, Direction dir, float distanceLimit, int damage, bool phasing = false)
            : base(new Vector2(0, 0), new Hitbox(0, 0))
        {
            this.gunPowerup = gunPowerup;
            this.textureName = textureName;
            this.setTexture(textureName);
            this.damage = damage;
            this.distanceLimit = distanceLimit;
            this.phasing = phasing;
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

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            LevelManager levelManager = RetroGame.TopLevelManagerScreen.levelManager;

            trailEmitter.position = position;
            trailEmitter.Update(gameTime);
            if (dying)
            {
                trailEmitter.active = false;
                explosionEmitter.Update(gameTime);
                if (explosionEmitter.isFinished() && trailEmitter.isFinished())
                {
                    gunPowerup.bulletsToRemove.Add(this);
                }
            }
            else
            {
                movement = velocity * seconds;
                float nextX = this.position.X + movement.X;
                float nextY = this.position.Y + movement.Y;
                distance += movement.Length();

                bool collided = false;
                if ((levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) ||
                            levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) ||
                            levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) ||
                            levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y))))
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
                
                int levelX = (int)position.X / Level.TEX_SIZE; // get which level you are in
                int levelY = (int)position.Y / Level.TEX_SIZE;
                Level l = RetroGame.getLevels()[levelX, levelY];
                if (l != null)
                {
                    foreach (Enemy e in RetroGame.getLevels()[levelX, levelY].enemies)
                        if (!e.dying && !enemiesAlreadyHit.Contains(e) && hitbox.intersects(e.hitbox))
                        {
                            collideWith(e);
                        }
                }

                explosionEmitter.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public void collideWith(Enemy e)
        {
            if (e != null)
            {
                if (dying)
                    return;
                e.hitBy(gunPowerup.hero, damage);
                SoundManager.PlaySoundOnce("EnemyHit", playInReverseDuringReverse: true);
                if (phasing)
                    enemiesAlreadyHit.Add(e);
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

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new BulletMemento(this);
        }

        private class BulletMemento : IMemento
        {
            public Object Target { get; set; }
            Vector2 position;
            float distance;
            Enemy[] alreadyHit;
            bool dying;
            IMemento trailEmitterMemento;
            IMemento explosionEmitterMemento;

            public BulletMemento(Bullet target)
            {
                Target = target;
                position = target.position;
                distance = target.distance;
                alreadyHit = target.enemiesAlreadyHit.ToArray();
                dying = target.dying;
                trailEmitterMemento = target.trailEmitter.GenerateMementoFromCurrentFrame();
                explosionEmitterMemento = target.explosionEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Bullet target = (Bullet)Target;
                if (nextFrame != null)
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    BulletMemento next = (BulletMemento)nextFrame;
                    target.position = position * thisInterp + next.position * nextInterp;
                    trailEmitterMemento.Apply(interpolationFactor, isNewFrame, ((BulletMemento)nextFrame).trailEmitterMemento);
                    explosionEmitterMemento.Apply(interpolationFactor, isNewFrame, ((BulletMemento)nextFrame).explosionEmitterMemento);
                }
                else
                {
                    target.position = position;
                    trailEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                    explosionEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                target.distance = distance;
                target.enemiesAlreadyHit = alreadyHit.ToList();
                target.dying = dying;
            }
        }
    }
}