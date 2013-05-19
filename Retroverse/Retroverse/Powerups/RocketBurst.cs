using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class RocketBurst : BoostPowerup
    {
        public Emitter leftBoosterFiring;
        public Emitter rightBoosterFiring;
        public Emitter leftBoosterIdle;
        public Emitter rightBoosterIdle;

        public const float BURST_SPEED_MULTIPLIER = 2f;

        public RocketBurst(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Rocket";
            SpecificName = "Burst";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("boosticon1");
            DrawBeforeHero = true;
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.DodgerBlue; //what color should this powerup's icon and related effects be?
            Description = "Activate for a strong\ntemporary boost in\nmovement speed"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            leftBoosterFiring = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            rightBoosterFiring = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            leftBoosterIdle = Emitter.getPrebuiltEmitter(PrebuiltEmitter.IdleBoostFire);
            rightBoosterIdle = Emitter.getPrebuiltEmitter(PrebuiltEmitter.IdleBoostFire);
            leftBooster = leftBoosterIdle;
            rightBooster = rightBoosterIdle;
            moveSpeedMultiplier = 1f;
        }

        public override void Activate(InputAction activationAction)
        {
            if (!bursting && burstRecharge >= BURST_COOLDOWN)
            {
                bursting = true;
                moveSpeedMultiplier = BURST_SPEED_MULTIPLIER;
                timeInBurst = 0;
                SoundManager.PlaySoundOnce("RocketBurst", playInReverseDuringReverse: true);
            }
        }

        public override void  Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            if (hero.Alive)
            {
                if (bursting)
                {
                    leftBoosterFiring.active = true;
                    rightBoosterFiring.active = true;
                    leftBoosterIdle.active = false;
                    rightBoosterIdle.active = false;
                }
                else
                {
                    leftBoosterIdle.active = true;
                    rightBoosterIdle.active = true;
                    leftBoosterFiring.active = false;
                    rightBoosterFiring.active = false;
                }
            }
            else
            {
                leftBoosterIdle.active = false;
                rightBoosterIdle.active = false;
                leftBoosterFiring.active = false;
                rightBoosterFiring.active = false;
            }

            if (bursting)
            {
                float speed = hero.movement.Length();
                leftBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (Hero.MOVE_SPEED * seconds));
                rightBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (Hero.MOVE_SPEED * seconds));
                hero.globalMoveSpeedMultiplier *= moveSpeedMultiplier;
            }

            if (burstRecharge >= BURST_COOLDOWN)
            {
                leftBoosterIdle.valueToDeath = 12;
                rightBoosterIdle.valueToDeath = 12;
                leftBoosterIdle.startColor = BOOST_IDLE_RECHARGED_COLOR;
                rightBoosterIdle.startColor = BOOST_IDLE_RECHARGED_COLOR;
            }
            else
            {
                leftBoosterIdle.valueToDeath = 10;
                rightBoosterIdle.valueToDeath = 10;
                leftBoosterIdle.startColor = BOOST_IDLE_NOT_RECHARGED_COLOR;
                rightBoosterIdle.startColor = BOOST_IDLE_NOT_RECHARGED_COLOR;
            }

            base.Update(gameTime);
            leftBoosterFiring.angle = boosterAngle;
            rightBoosterFiring.angle = boosterAngle;

            if (seconds > 0)
            {
                leftBoosterFiring.Update(gameTime);
                rightBoosterFiring.Update(gameTime);
            }
            Vector2 nextHeroPos = hero.position + hero.movement; //use next frame's hero position to stick the particle emitter to the hero better
            leftBoosterFiring.position = nextHeroPos + leftBoosterOffset;
            rightBoosterFiring.position = nextHeroPos + rightBoosterOffset;
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            if (bursting) charge = 0;
            else charge = burstRecharge / BURST_COOLDOWN;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            leftBoosterIdle.Draw(spriteBatch);
            rightBoosterIdle.Draw(spriteBatch);
            leftBoosterFiring.Draw(spriteBatch);
            rightBoosterFiring.Draw(spriteBatch);
        }
        public override IMemento GenerateMementoFromCurrentFrame()
        {
            return new RocketBurstMemento(this);
        }

        protected class RocketBurstMemento : BoostMemento
        {
            //add necessary fields to save information here
            IMemento leftFiringMemento;
            IMemento rightFiringMemento;

            public RocketBurstMemento(RocketBurst target)
                : base(target)
            {
                leftFiringMemento = target.leftBoosterFiring.GenerateMementoFromCurrentFrame();
                rightFiringMemento = target.rightBoosterFiring.GenerateMementoFromCurrentFrame();
            }

            public override void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                base.Apply(interpolationFactor, isNewFrame, nextFrame);

                if (nextFrame != null)
                {
                    leftFiringMemento.Apply(interpolationFactor, isNewFrame, ((RocketBurstMemento)nextFrame).leftFiringMemento);
                    rightFiringMemento.Apply(interpolationFactor, isNewFrame, ((RocketBurstMemento)nextFrame).rightFiringMemento);
                }
                else
                {
                    leftFiringMemento.Apply(interpolationFactor, isNewFrame, null);
                    rightFiringMemento.Apply(interpolationFactor, isNewFrame, null);
                }
            }
        }
    }
}
