using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class Flame : Entity
    {
        public Emitter flameEmitter;
        public float damagePerSecond;
        public float offset;
        Hero hero;

        public Flame(Hero hero, float damagePerSecond, float offset)
            : base(new Vector2(hero.position.X, hero.position.Y), new Hitbox(32, 32))
        {
            active = false;
            this.hero = hero;
            this.offset = offset;
            setTexture("flame");
            this.damagePerSecond = damagePerSecond;
            flameEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.FlameFire);
        }

        public void collideWith(Enemy e)
        {
            if (e != null)
            {
                e.health -= damagePerSecond;
                if (e.health <= 0)
                {
                    e.dieFromHero(hero);
                }
            }
        }

        public void Update(GameTime gameTime, bool collide)
        {
            active = collide;
            switch (hero.direction)
            {
                case Direction.Up:
                    position.X = hero.position.X;
                    position.Y = hero.position.Y - offset;
                    break;
                case Direction.Down:
                    position.X = hero.position.X;
                    position.Y = hero.position.Y + offset;
                    break;
                case Direction.Left:
                    position.X = hero.position.X - offset;
                    position.Y = hero.position.Y;
                    break;
                case Direction.Right:
                    position.X = hero.position.X + offset;
                    position.Y = hero.position.Y;
                    break;
            }

            updateCurrentLevelAndTile();
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            if (active)
                foreach (Enemy e in RetroGame.getLevels()[levelX, levelY].enemies)
                    if (hitbox.intersects(e.hitbox))
                    {
                        e.hitBy(hero, damagePerSecond * seconds);
                    }

            flameEmitter.position = position;
            flameEmitter.active = collide;
            flameEmitter.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            flameEmitter.Draw(spriteBatch);
        }
    }
}
