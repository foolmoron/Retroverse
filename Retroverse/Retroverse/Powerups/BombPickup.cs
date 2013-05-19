using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class BombPickup : Powerup
    {
        public BombPickup(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = Powerups.INSTANT;
            SpecificName = "Bomb";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            GemCost = COST_MEDIUM; //how many gems does it take to buy this from the store?
            Icon = TextureManager.Get("bomb"); //filename for this powerup's icon
            DrawBeforeHero = true; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.Black; //what color should this powerup's icon and related effects be?
            Description = "+1 Bomb"; //give a short description (with appropriate newlines) of the powerup, for display to the player
        }

        public override void OnAddedToHero()
        {
            RetroGame.AddBomb();
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
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            //Powerup logic here
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            //Calculate powerup's current charge level (0.0-1.0)
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Powerup drawing here
        }
    }
}