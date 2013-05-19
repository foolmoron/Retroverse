
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class RescuePowerup : CoOpPowerup
    {
        public float rescueTimer;
        Emitter rescueEmitter, endEmitter;
        public const float RESCUE_INTERVAL = 5.0f;     

        public RescuePowerup(Hero hero, Hero otherHero)
            : base(hero, otherHero)
        {
            rescueTimer = 0f;
			/* Set these properties for your specific powerup */
			GenericName = "Rescue";
			SpecificName = "Rescue";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("warptopartner"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.Yellow; //what color should this powerup's icon and related effects be?
            Description = "Instantly teleport to\nyour partner";
            GemCost = 0; //how many gems does it take to buy this from the store?
        }

        private void initializeRescueEmitter()
        {
            rescueEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.CollectedSparks);
            //blinkEmitter.startColor = Color.Red;
            //blinkEmitter.endColor = Color.Purple;
            rescueEmitter.active = false;
        }

        private void initializeEndEmitter()
        {
            endEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
            //endEmitter.startColor = Color.Purple;
            //endEmitter.endColor = Color.Red;
            endEmitter.maxParticlesToEmit = 50;
            endEmitter.active = false;
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
            if (otherHero == null || !hero.Alive || !otherHero.Alive)
                return;

            if (rescueTimer >= RESCUE_INTERVAL)
            {
                rescueTimer = 0;
                rescueEmitter.active = true;
                hero.position = otherHero.position;
                hero.direction = otherHero.direction;
                hero.updateCurrentLevelAndTile();
                hero.teleportedThisFrame = true;
                endEmitter.position = hero.position;
                endEmitter.active = true;
                SoundManager.PlaySoundOnce("RescueTeleport", playInReverseDuringReverse: true);
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            rescueTimer += seconds * hero.powerupCooldownModifier; //keeps track of cooldown

            if (rescueEmitter == null)
            {
                initializeRescueEmitter();
                rescueEmitter.active = false;
            }
            else
            {
                if (!rescueEmitter.active)
                    rescueEmitter.position = hero.position; //updates particle emitter
                rescueEmitter.Update(gameTime);
                if (rescueEmitter.isFinished())
                    rescueEmitter = null;
            }

            if (endEmitter == null)
            {
                initializeEndEmitter();
            }
            else
            {
                endEmitter.Update(gameTime);
                if (endEmitter.isFinished())
                    endEmitter = null;
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            if(otherHero == null || !otherHero.Alive)
                charge = 0;
            else
                charge = rescueTimer / RESCUE_INTERVAL;
            return charge;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (endEmitter != null)
                endEmitter.Draw(spriteBatch);
            if (rescueEmitter != null)
                rescueEmitter.Draw(spriteBatch); //just draw the emitter    
        }
    }
}