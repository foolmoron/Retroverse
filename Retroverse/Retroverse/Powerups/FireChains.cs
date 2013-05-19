using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Particles;

namespace Retroverse
{
    public class FireChains : CoOpPowerup, IReversible
    {
        public const float MAX_ACTIVATION_DISTANCE = 200f;
        public const float MIN_ACTIVATION_DISTANCE = 50f;
        public const float MAX_DAMAGE_PER_SECOND = 20f;
        public const float MIN_DAMAGE_PER_SECOND = 5f;
        public const float MAX_DAMAGE_DISTANCE = MAX_ACTIVATION_DISTANCE / 2;
        public const float MIN_DAMAGE_DISTANCE = MAX_ACTIVATION_DISTANCE;

        public const float MIN_BAR_WIDTH = 2f;
        public const float MAX_BAR_WIDTH = 10f;

        public const float INITIAL_PARTICLE_SIZE_MAX = 0.5f;
        public const float INITIAL_PARTICLE_SIZE_MIN = 0.25f;

        public float strength;
        public bool active = false;
        public int diagonalDistance;
        public float rotation;

        public const float DRAW_OFFSET = 16;
        public float drawRotation;
        public Vector2 drawPosA, drawPosB;

        public LineEmitter chainsEmitter;

        public FireChains(Hero hero, Hero otherHero)
            : base(hero, otherHero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Chains";
            SpecificName = "Fire";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("firechain"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = new Color(255, 100, 100); //what color should this powerup's icon and related effects be?
            Description = "Forms a damaging\nchain between two players\nwhen close to each other";
            GemCost = COST_VERYEXPENSIVE; //how many gems does it take to buy this from the store?

            chainsEmitter = LineEmitter.getPrebuiltEmitter(PrebuiltLineEmitter.FireChainsFire);
        }

        public override void OnAddedToHero()
        {
            //Logic when added to hero here
        }

        public override void OnRemovedFromHero()
        {
            //Logic when removed from hero here
        }

        public override void Activate(InputAction activationAction)
        {
            //Activate logic here
        }

        public override void Update(GameTime gameTime)
        {
            if (otherHero != null && hero.Alive && otherHero.Alive)
            {
                float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
                UpdateStrengthAndActivity();
                if (active)
                {
                    float damagePerSecond = strength * (MAX_DAMAGE_PER_SECOND - MIN_DAMAGE_PER_SECOND) + MIN_DAMAGE_PER_SECOND;
                    foreach (Enemy e in RetroGame.TopLevelManagerScreen.levelManager.levels[hero.levelX, hero.levelY].enemies)
                    {
                        if (!e.dying && e.hitbox.intersectsLine(hero.position, otherHero.position, 2))
                        {
                            e.hitBy(null, damagePerSecond * seconds);
                        }
                    }
                    chainsEmitter.position = drawPosA;
                    chainsEmitter.positionB = drawPosB;
                }
                chainsEmitter.startSize = strength * (INITIAL_PARTICLE_SIZE_MAX - INITIAL_PARTICLE_SIZE_MIN) + INITIAL_PARTICLE_SIZE_MIN;
                chainsEmitter.active = active;
            }
            else
            {
                chainsEmitter.active = false;
            }
            chainsEmitter.Update(gameTime);
        }

        public void UpdateStrengthAndActivity()
        {
            float distance = Vector2.Distance(hero.position, otherHero.position);
            active = (distance <= MAX_ACTIVATION_DISTANCE && distance >= MIN_ACTIVATION_DISTANCE) && (hero.levelX == otherHero.levelX && hero.levelY == otherHero.levelY);
            if (active)
            {
                strength = (distance > MAX_DAMAGE_DISTANCE) ? 1 - ((distance - MAX_DAMAGE_DISTANCE) / (MIN_DAMAGE_DISTANCE - MAX_DAMAGE_DISTANCE)) : 1;
            }

            float distX = hero.position.X - otherHero.position.X;
            float distY = hero.position.Y - otherHero.position.Y;
            rotation = (float)Math.Atan2(distY, distX);
            drawRotation = (float)(rotation + Math.PI);

            drawPosA.X = hero.position.X + ((float)Math.Cos(drawRotation) * DRAW_OFFSET);
            drawPosA.Y = hero.position.Y + ((float)Math.Sin(drawRotation) * DRAW_OFFSET);
            drawPosB.X = otherHero.position.X - ((float)Math.Cos(drawRotation) * DRAW_OFFSET);
            drawPosB.Y = otherHero.position.Y - ((float)Math.Sin(drawRotation) * DRAW_OFFSET);

            diagonalDistance = (int)Vector2.Distance(drawPosA, drawPosB);
        }

        public override float GetPowerupCharge()
        {
            float charge = 1;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!hero.Alive || !otherHero.Alive)
                return;
            Color lineColor = Color.Lerp(Color.White, Color.Red, strength);
            float width = strength * (MAX_BAR_WIDTH - MIN_BAR_WIDTH) + MIN_BAR_WIDTH;

            Rectangle rec = new Rectangle((int)drawPosA.X, (int)(drawPosA.Y), diagonalDistance, (int)width);
            if (active)
                spriteBatch.Draw(RetroGame.PIXEL, rec, null, lineColor.withAlpha(200), drawRotation, new Vector2(0, 0.5f), SpriteEffects.None, 0);
            chainsEmitter.Draw(spriteBatch);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new FireChainsMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class FireChainsMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            IMemento chainsEmitterMemento;

            public FireChainsMemento(FireChains target)
            {
                //save necessary information from target here
                Target = target;
                chainsEmitterMemento = target.chainsEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                ((FireChains)Target).UpdateStrengthAndActivity();
                if (nextFrame != null)
                {
                    chainsEmitterMemento.Apply(interpolationFactor, isNewFrame, ((FireChainsMemento)nextFrame).chainsEmitterMemento);
                }
                else
                {
                    chainsEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
            }
        }
    }
}