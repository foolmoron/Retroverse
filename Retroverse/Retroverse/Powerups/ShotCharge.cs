using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class ShotCharge : GunPowerup
    {
        public static readonly Color EMITTER_CHARGE_COLOR = new Color(204, 185, 67, 255);
        public const int BULLET_DAMAGE_CHARGE_SMALL = 1;
        public const int BULLET_DAMAGE_CHARGE_MEDIUM = 3;
        public const int BULLET_DAMAGE_CHARGE_LARGE = 4;
        public const float BULLET_CHARGE_TIME_SMALL = 0.25f; //secs
        public const float BULLET_CHARGE_TIME_MEDIUM = 1f; //secs
        public const float BULLET_CHARGE_TIME_LARGE = 2f; //secs
        public static readonly Color CHARGE_COLOR_SMALL = new Color(248, 180, 100, 255);
        public static readonly Color CHARGE_COLOR_MEDIUM = new Color(248, 248, 56, 255);
        public static readonly Color CHARGE_COLOR_LARGE = new Color(248, 248, 175, 255);
        public const int BASE_BULLET_SIZE = 64;
        public const float BULLET_SMALL_SCALE = 0.25f;
        public const float BULLET_MEDIUM_SCALE = 0.5f;
        public const float BULLET_LARGE_SCALE = 1f;
        public const float CHARGE_PARTICLES_SMALL_SCALE = 0.25f;
        public const float CHARGE_PARTICLES_MEDIUM_SCALE = 0.4f;
        public const float CHARGE_PARTICLES_LARGE_SCALE = 0.7f;
        public Emitter chargeEmitter;

        public float chargeTimer = 0;

        public ShotCharge(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Gun";
            SpecificName = "Charge Shot";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("chargeshoticon2"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.Goldenrod; //what color should this powerup's icon and related effects be?
            Description = "A slow firing gun that\ncharges up when held"; //give a short description (with appropriate newlines) of the powerup, for display to the player
            GemCost = COST_EXPENSIVE;

            chargeEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
        }

        public override void Activate(InputAction activationAction)
        {
            base.Activate(activationAction);
        }

        public override void Update(GameTime gameTime)
        {
            base.PreUpdate(gameTime);

            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);

            if (activated)
            {
                chargeTimer += seconds*hero.powerupCooldownModifier;
            }
            chargeEmitter.active = activated;
            if (chargeTimer < BULLET_CHARGE_TIME_SMALL)
            {
                chargeEmitter.active = false;
            }
            else if (chargeTimer >= BULLET_CHARGE_TIME_SMALL && chargeTimer < BULLET_CHARGE_TIME_MEDIUM)
            {
                if (!activated)
                {
                    Bullet b = new Bullet(this, "chargebullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_CHARGE_COLOR, hero.direction, Bullet.DISTANCE_LIMIT_CHARGE, (int)(BULLET_DAMAGE_CHARGE_SMALL * damageModifier));
                    ammo.Add(b);
                    b.scale = BULLET_SMALL_SCALE;
                    b.hitbox.originalRectangle.Height = (int)(BASE_BULLET_SIZE * BULLET_SMALL_SCALE);
                    b.hitbox.originalRectangle.Width = (int)(BASE_BULLET_SIZE * BULLET_SMALL_SCALE);
                    b.position = new Vector2(hero.position.X, hero.position.Y);
                    b.explosionEmitter.startSize = 1f;
                    b.explosionEmitter.endSize = 1f;
                    chargeTimer = 0;

                    FiredSound = "BulletTiny";
                    shotFired = true;
                }
                chargeEmitter.startSize = CHARGE_PARTICLES_SMALL_SCALE;
                Color c = CHARGE_COLOR_SMALL;
                chargeEmitter.startColor = c;
                c.A = 255;
                chargeEmitter.endColor = c;
            }
            else if (chargeTimer >= BULLET_CHARGE_TIME_MEDIUM && chargeTimer < BULLET_CHARGE_TIME_LARGE)
            {
                if (!activated)
                {
                    Bullet b = new Bullet(this, "chargebullet2", PrebuiltEmitter.MediumBulletSparks, EMITTER_CHARGE_COLOR, hero.direction, Bullet.DISTANCE_LIMIT_CHARGE, (int)(BULLET_DAMAGE_CHARGE_MEDIUM * damageModifier), true);
                    ammo.Add(b);
                    b.scale = BULLET_MEDIUM_SCALE;
                    b.hitbox.originalRectangle.Height = (int)(BASE_BULLET_SIZE * BULLET_MEDIUM_SCALE);
                    b.hitbox.originalRectangle.Width = (int)(BASE_BULLET_SIZE * BULLET_MEDIUM_SCALE);
                    b.position = new Vector2(hero.position.X, hero.position.Y);
                    b.explosionEmitter.startSize = 1f;
                    b.explosionEmitter.endSize = 1f;
                    chargeTimer = 0;

                    FiredSound = "BulletLight";
                    shotFired = true;
                }
                chargeEmitter.startSize = CHARGE_PARTICLES_MEDIUM_SCALE;
                Color c = CHARGE_COLOR_MEDIUM;
                chargeEmitter.startColor = c;
                c.A = 255;
                chargeEmitter.endColor = c;
            }
            else if (chargeTimer >= BULLET_CHARGE_TIME_LARGE)
            {
                if (!activated)
                {
                    Bullet b = new Bullet(this, "chargebullet3", PrebuiltEmitter.LargeBulletSparks, EMITTER_CHARGE_COLOR, hero.direction, Bullet.DISTANCE_LIMIT_CHARGE, (int)(BULLET_DAMAGE_CHARGE_LARGE * damageModifier), true);
                    ammo.Add(b);
                    b.scale = BULLET_LARGE_SCALE;
                    b.hitbox.originalRectangle.Height = (int)(BASE_BULLET_SIZE * BULLET_LARGE_SCALE);
                    b.hitbox.originalRectangle.Width = (int)(BASE_BULLET_SIZE * BULLET_LARGE_SCALE);
                    b.position = new Vector2(hero.position.X, hero.position.Y);
                    b.explosionEmitter.startSize = 1f;
                    b.explosionEmitter.endSize = 1f;
                    chargeTimer = 0;

                    FiredSound = "BulletStrong";
                    shotFired = true;
                }
                chargeEmitter.startSize = CHARGE_PARTICLES_LARGE_SCALE;
                Color c = CHARGE_COLOR_LARGE;
                chargeEmitter.startColor = c;
                c.A = 255;
                chargeEmitter.endColor = c;
            }

            chargeEmitter.position = hero.position;
            if (seconds > 0)
            {
                chargeEmitter.Update(gameTime);
            }

            base.PostUpdate(gameTime);
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            if (chargeTimer < BULLET_CHARGE_TIME_SMALL)
                charge = chargeTimer / BULLET_CHARGE_TIME_SMALL;
            else if (chargeTimer >= BULLET_CHARGE_TIME_SMALL && chargeTimer < BULLET_CHARGE_TIME_MEDIUM)
                charge = (chargeTimer - BULLET_CHARGE_TIME_SMALL) / (BULLET_CHARGE_TIME_MEDIUM - BULLET_CHARGE_TIME_SMALL);
            else if (chargeTimer >= BULLET_CHARGE_TIME_MEDIUM && chargeTimer < BULLET_CHARGE_TIME_LARGE)
                charge = (chargeTimer - BULLET_CHARGE_TIME_MEDIUM) / (BULLET_CHARGE_TIME_LARGE - BULLET_CHARGE_TIME_MEDIUM);
            else if (chargeTimer >= BULLET_CHARGE_TIME_LARGE)
                charge = 1;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            chargeEmitter.Draw(spriteBatch);
        }

        public override IMemento GenerateMementoFromCurrentFrame()
        {
            return new ShotChargeMemento(this);
        }

        protected class ShotChargeMemento : GunPowerupMemento
        {
            //add necessary fields to save information here
            IMemento chargeEmitterMemento;

            public ShotChargeMemento(ShotCharge target)
                : base(target)
            {
                chargeEmitterMemento = target.chargeEmitter.GenerateMementoFromCurrentFrame();
            }

            public override void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                base.Apply(interpolationFactor, isNewFrame, nextFrame);

                if (nextFrame != null)
                {
                    chargeEmitterMemento.Apply(interpolationFactor, isNewFrame, ((ShotChargeMemento)nextFrame).chargeEmitterMemento);
                }
                else
                {
                    chargeEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
            }
        }
    }
}