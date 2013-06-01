using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class RetroPort : Powerup
    {
        public const int SAND_ADDED_ON_COLLECT = 2;

        public RetroPort(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Retro";
            SpecificName = "Reverse";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("retroicon1"); //filename for this powerup's icon
            DrawBeforeHero = true; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = new Color(25, 25, 25); //what color should this powerup's icon and related effects be?
            Description = "Rewinds the game\nfor a short time"; //give a short description (with appropriate newlines) of the powerup, for display to the player
        }

        public override void OnCollectedByHero(Hero collector)
        {
            for (int i = 0; i < SAND_ADDED_ON_COLLECT; i++)
            {
                RetroGame.AddSand();
            }
        }

        public override void OnAddedToHero()
        {
            //Logic when added to hero here
        }

        public override void OnRemovedFromHero()
        {
            //Logic when removed from hero here
        }

        public override void Activate(InputAction action)
        {
            switch (RetroGame.State)
            {
                case GameState.Arena:
                case GameState.Escape:
                    if (RetroGame.AvailableSand > 0 && History.CanRevert())
                    {
                        History.ActivateRevert(hero, action);
                        RetroGame.RemoveSand();
                    }
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            //Powerup logic here
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            if (RetroGame.AvailableSand > 0)
                charge = History.secsSinceLastRetroPort / History.RETROPORT_BASE_SECS;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (History.LastHistory != null)
                History.LastHistory.AttemptDrawHero(spriteBatch, hero);
        }
    }
}