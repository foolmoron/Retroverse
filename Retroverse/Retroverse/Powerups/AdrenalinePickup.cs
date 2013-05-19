using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    class AdrenalinePickup : Adrenaline, IReversible
    {

        public const float ADRENALINE_TIME = 15f; //How long the powerup lasts (seconds)
        public bool activated = false;
        public float timer = 0;
        public Emitter adrenalineEmitter;

        public AdrenalinePickup(Hero hero)
            : base(hero)
        {
            GenericName = "Adrenaline";
            SpecificName = "Pickup";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            GemCost = COST_CHEAP; //how many gems does it take to buy this from the store?
            DrawBeforeHero = false;
            Icon = TextureManager.Get("adrenalinepickup");
            Description = "Temporarily reduces\ncooldown times when\nactivated";
            TintColor = Color.GreenYellow;
            
            modifier = 2f;
            adrenalineEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
            adrenalineEmitter.startColor = Color.Yellow;
            adrenalineEmitter.endColor = Color.Red;
        }


        public override void Activate(InputAction activationAction)
        {
            activated = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!hero.Alive)
            {
                toRemove = true;
                return;
            }
            if (activated) {
                float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
                if (timer < ADRENALINE_TIME) {
                    hero.powerupCooldownModifier *= modifier;
                    timer += seconds;
                }
                else {
                    toRemove = true;
                }

                adrenalineEmitter.position = hero.position; //updates particle emitter
                adrenalineEmitter.Update(gameTime);
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = ((ADRENALINE_TIME-timer)/ timer);
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (activated)
                adrenalineEmitter.Draw(spriteBatch);
        }
        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new AdrenalinePickupMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class AdrenalinePickupMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float timer = 0f;
            bool activated = false;
            IMemento adrenalineEmitterMemento;

            public AdrenalinePickupMemento(AdrenalinePickup target)
            {
                //save necessary information from target here
                Target = target;
                timer = target.timer;
                activated = target.activated;
                adrenalineEmitterMemento = target.adrenalineEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                AdrenalinePickup target = (AdrenalinePickup)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    AdrenalinePickupMemento next = (AdrenalinePickupMemento)nextFrame;
                    target.timer = timer * thisInterp + next.timer * nextInterp;
                    adrenalineEmitterMemento.Apply(interpolationFactor, isNewFrame, next.adrenalineEmitterMemento);
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.timer = timer;
                    adrenalineEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                target.activated = activated;
            }
        }
    }
}
