using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public abstract class BoostPowerup : Powerup, IReversible
    {
        public Emitter leftBooster;
        public Vector2 leftBoosterOffset;
        public Emitter rightBooster;
        public Vector2 rightBoosterOffset;
        public float boosterAngle = 0f;

        public static readonly float BOOSTER_LENGTH = 12;
        public float moveSpeedMultiplier = 1f;
        public const float BURST_DURATION = 1f; //secs
        public const float BURST_COOLDOWN = 2.5f; //secs
        public float burstTimer = 0;
        public float burstRecharge = 0;
        public float timeInBurst = 0;
        public bool bursting = false;
        public static readonly Color BOOST_IDLE_RECHARGED_COLOR = new Color(0, 128, 255, 255);
        public static readonly Color BOOST_IDLE_NOT_RECHARGED_COLOR = new Color(255, 140, 0, 255);
        
        public BoostPowerup(Hero hero)
            : base(hero)
        {
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);

            if (bursting)
            {
                timeInBurst += seconds;
                if (timeInBurst >= BURST_DURATION)
                {
                    moveSpeedMultiplier = 1f;
                    burstRecharge = 0;
                    bursting = false;
                }
            }
            else if (burstRecharge < BURST_COOLDOWN)
            {
                burstRecharge += seconds*hero.powerupCooldownModifier;
            }

            if (Hero.HERO_TIMESCALE > 0f)
            {
                switch (hero.direction)
                {
                    case Direction.Up:
                        leftBoosterOffset = new Vector2(-6, 12);
                        rightBoosterOffset = new Vector2(6, 12);
                        boosterAngle = (float)Math.PI / 2;
                        break;
                    case Direction.Down:
                        leftBoosterOffset = new Vector2(6, -12);
                        rightBoosterOffset = new Vector2(-6, -12);
                        boosterAngle = (float)Math.PI * 3 / 2;
                        break;
                    case Direction.Left:
                        leftBoosterOffset = new Vector2(12, 6);
                        rightBoosterOffset = new Vector2(12, -6);
                        boosterAngle = 0;
                        break;
                    case Direction.Right:
                        leftBoosterOffset = new Vector2(-12, -6);
                        rightBoosterOffset = new Vector2(-12, 6);
                        boosterAngle = (float)Math.PI;
                        break;
                }
            }

            leftBooster.angle = boosterAngle;
            rightBooster.angle = boosterAngle;

            if (seconds > 0)
            {
                leftBooster.Update(gameTime);
                rightBooster.Update(gameTime);
            }
            Vector2 nextHeroPos = hero.position + hero.movement; //use next frame's hero position to stick the particle emitter to the hero better
            leftBooster.position = nextHeroPos + leftBoosterOffset;
            rightBooster.position = nextHeroPos + rightBoosterOffset;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            leftBooster.Draw(spriteBatch);
            rightBooster.Draw(spriteBatch);
        }

        public virtual IMemento GenerateMementoFromCurrentFrame()
        {
            return new BoostMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class BoostMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float moveSpeedMultiplier;
            float burstTimer;
            float burstRecharge;
            float timeInBurst;
            bool bursting;

            IMemento leftBoosterMemento;
            IMemento rightBoosterMemento;

            public BoostMemento(BoostPowerup target)
            {
                //save necessary information from target here
                Target = target;
                moveSpeedMultiplier = target.moveSpeedMultiplier;
                burstTimer = target.burstTimer;
                burstRecharge = target.burstRecharge;
                timeInBurst = target.timeInBurst;
                bursting = target.bursting;
                leftBoosterMemento = target.leftBooster.GenerateMementoFromCurrentFrame();
                rightBoosterMemento = target.rightBooster.GenerateMementoFromCurrentFrame();
            }

            public virtual void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                BoostPowerup target = (BoostPowerup)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    BoostMemento next = (BoostMemento)nextFrame;
                    target.burstTimer = burstTimer * thisInterp + next.burstTimer * nextInterp;
                    target.burstRecharge = burstRecharge * thisInterp + next.burstRecharge * nextInterp;
                    target.timeInBurst = timeInBurst * thisInterp + next.timeInBurst * nextInterp;
                    leftBoosterMemento.Apply(interpolationFactor, isNewFrame, next.leftBoosterMemento);
                    rightBoosterMemento.Apply(interpolationFactor, isNewFrame, next.rightBoosterMemento);
                }
                else
                {
                    //do non-interpolative versions of the above applications herew
                    target.burstTimer = burstTimer;
                    target.burstRecharge = burstRecharge;
                    target.timeInBurst = timeInBurst;
                    leftBoosterMemento.Apply(interpolationFactor, isNewFrame, null);
                    rightBoosterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                //apply values that never need interpolation here
                target.moveSpeedMultiplier = moveSpeedMultiplier;
                target.bursting = bursting;
            }
        }
    }
}
