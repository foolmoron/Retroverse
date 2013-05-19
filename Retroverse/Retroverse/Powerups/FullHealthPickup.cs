using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class FullHealthPickup : Powerup
    {
        public FullHealthPickup(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Health";
            SpecificName = "Full Revive";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            GemCost = COST_VERYEXPENSIVE; //how many gems does it take to buy this from the store?
            Icon = TextureManager.Get("healthicon"); //filename for this powerup's icon
            DrawBeforeHero = true; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.Gold; //what color should this powerup's icon and related effects be?
            Description = "Survive a killing blow\nonce with all health\n restored"; //give a short description (with appropriate newlines) of the powerup, for display to the player
        }

        public override void OnAddedToHero()
        {
        }

        public override void OnRemovedFromHero()
        {
            //Logic when removed from hero here
        }

        public override void Activate(InputAction activationAction)
        {
            hero.health = Hero.INITIAL_HEALTH;
            toRemove = true;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            //Powerup logic here
        }

        public override float GetPowerupCharge()
        {
            float charge = 1;
            //Calculate powerup's current charge level (0.0-1.0)
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Powerup drawing here
        }
    }
}