using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Particles;

namespace Retroverse
{
    public class TimedSpeedBoost : Powerup, IReversible
    {
        public const float SPEED_BOOST = 1.5f;
        public const float BOOST_SECS = 10f;
        public float secsPassed = 0f;
        public bool activated = false;
        public Emitter speedEmitter;

        public TimedSpeedBoost(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Speed";
            SpecificName = "Pickup";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("fastforward"); //filename for this powerup's icon
            DrawBeforeHero = true; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.Green; //what color should this powerup's icon and related effects be?
            Description = "Temporarily speed\n yourself up once"; //give a short description (with appropriate newlines) of the powerup, for display to the player
            GemCost = COST_CHEAP;

            speedEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
            speedEmitter.startColor = Color.LawnGreen;
            speedEmitter.endColor = Color.LightSkyBlue;
            speedEmitter.particlesPerSecond = 100;
        }

        public override void Activate(InputAction activationAction)
        {
            //Activate logic here
            activated = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!hero.Alive)
            {
                toRemove = true;
                return;
            }

            if (activated)
            {
                float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
                if (secsPassed < BOOST_SECS)
                {
                    secsPassed += seconds;
                    hero.globalMoveSpeedMultiplier *= SPEED_BOOST;
                }
                else
                    toRemove = true;

                speedEmitter.position = hero.position; //updates particle emitter
                speedEmitter.Update(gameTime);
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = (BOOST_SECS - secsPassed) / BOOST_SECS;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (activated)
                speedEmitter.Draw(spriteBatch);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new TimedSpeedBoostMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class TimedSpeedBoostMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float secsPassed = 0f;
            bool activated = false;
            IMemento speedEmitterMemento;

            public TimedSpeedBoostMemento(TimedSpeedBoost target)
            {
                //save necessary information from target here
                Target = target;
                secsPassed = target.secsPassed;
                activated = target.activated;
                speedEmitterMemento = target.speedEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                TimedSpeedBoost target = (TimedSpeedBoost)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    TimedSpeedBoostMemento next = (TimedSpeedBoostMemento)nextFrame;
                    target.secsPassed = secsPassed * thisInterp + next.secsPassed * nextInterp;
                    speedEmitterMemento.Apply(interpolationFactor, isNewFrame, next.speedEmitterMemento);
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.secsPassed = secsPassed;
                    speedEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                target.activated = activated;
            }
        }
    }
}