using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class ShotForward : GunPowerup
    {
        public static readonly Color EMITTER_STRAIGHT_COLOR = new Color(207, 17, 17, 255);
        public const int BULLET_SIZE = 20;

        public ShotForward(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Gun";
            SpecificName = "Forward Shot";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("forwardshoticon1"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.OrangeRed; //what color should this powerup's icon and related effects be?
            Description = "A quick forward\nfiring gun"; //give a short description (with appropriate newlines) of the powerup, for display to the player
            GemCost = COST_EXPENSIVE;
            
            FiredSound = "BulletLight";
        }

        public override void Activate(InputAction activationAction)
        {
            base.Activate(activationAction);
            if (bulletTimer < BULLET_FIRE_INTERVAL)
                return;
            bulletTimer = 0;
            ammo.Add(new Bullet(this, "bullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_STRAIGHT_COLOR, hero.direction, Bullet.DISTANCE_LIMIT_NORMAL, (int)(BULLET_DAMAGE_NORMAL * damageModifier)));
            ammo.Last().position = new Vector2(hero.position.X, hero.position.Y);
            ammo.Last().scale = BULLET_NORMAL_SCALE;
            ammo.Last().hitbox.originalRectangle.Height = BULLET_SIZE;
            ammo.Last().hitbox.originalRectangle.Width = BULLET_SIZE;
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