using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class ShotBasic : GunPowerup
    {
        public static readonly Color EMITTER_BASIC_COLOR = new Color(204, 185, 67, 255);
        public const float BULLET_DAMAGE_BASIC = 2;
        public const float BULLET_BASIC_SCALE = 0.25f;
        public const float BULLET_BASIC_FIRE_INTERVAL = 0.3f;
        public const int BULLET_SIZE = 16;

        public ShotBasic(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Gun";
            SpecificName = "Basic Shot";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("chargeshoticon1"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.SandyBrown; //what color should this powerup's icon and related effects be?
            Description = "A weak gun, but it gets\nthe job done"; //give a short description (with appropriate newlines) of the powerup, for display to the player
            GemCost = 0;

            FiredSound = "BulletTiny";
        }

        public override void Activate(InputAction activationAction)
        {
            base.Activate(activationAction);
            if (bulletTimer < BULLET_BASIC_FIRE_INTERVAL)
                return;
            bulletTimer = 0;
            Bullet b = new Bullet(this, "chargebullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_BASIC_COLOR, hero.direction, Bullet.DISTANCE_LIMIT_CHARGE, (int)(BULLET_DAMAGE_BASIC * damageModifier));
            b.scale = BULLET_BASIC_SCALE;
            b.hitbox.originalRectangle.Height = BULLET_SIZE;
            b.hitbox.originalRectangle.Width = BULLET_SIZE;
            b.position = new Vector2(hero.position.X, hero.position.Y);
            b.explosionEmitter.startSize = 0.25f;
            b.explosionEmitter.endSize = 0.25f;
            ammo.Add(b);
            shotFired = true;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            base.Update(gameTime);
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            charge = bulletTimer / BULLET_FIRE_INTERVAL;
            return charge;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            //Powerup drawing here
            base.Draw(spriteBatch);
        }
    }
}