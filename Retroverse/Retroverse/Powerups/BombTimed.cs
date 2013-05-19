using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class BombTimed : BombPowerup
    {
        public const float EXPLOSION_TIME = 2f;

        public float[] TICK_STAGE_TIMES = new float[] {0.0f, 1.5f};
        public int tickStage = 0;
        public float[] TICK_INTERVALS = new float[] {0.4f, 0.2f};
        public float tickInterval;
        public float tickTimer = 0;

        public BombTimed(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Bomb";
            SpecificName = "Timed";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?            
            Icon = TextureManager.Get("bombtimed"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.Lime; //what color should this powerup's icon and related effects be?
            Description = "Requires bomb charge!\nPlaces a bomb that\nautomatically detonates\nafter a short time"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            BombInterval = 2.0f;
            ExplosionRadius = 3;
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
            if (bombTimer >= BombInterval && RetroGame.AvailableBombs > 0)
            {
                bombs.Add(new Bomb(this, hero.position, "bombtimed", EXPLOSION_TIME, ExplosionRadius));
                bombTimer = 0;
                RetroGame.RemoveBomb();
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            base.Update(gameTime);
            if (!hero.Alive)
                return;
            if (bombs.Count > 0)
            {
                tickTimer += seconds;
                for(tickStage = 0; tickStage < TICK_STAGE_TIMES.Length; tickStage++) //terribly ugly loop... oh well
                {
                    if (bombs[0].timeAlive < TICK_STAGE_TIMES[tickStage])
                        break;
                }
                tickStage--;
                Console.WriteLine("timealive=" + bombs[0].timeAlive + " stage=" + tickStage);
                tickInterval = TICK_INTERVALS[tickStage];
                if (tickTimer >= tickInterval)
                {
                    tickTimer = 0;
                    SoundManager.PlaySoundOnce("BombTick", playInReverseDuringReverse: true);
                }
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = base.GetPowerupCharge();
            //Calculate powerup's current charge level (0.0-1.0)
            return charge;
        }
    }
}