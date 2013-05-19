 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class BombSet : BombPowerup
    {
        public BombSet(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Bomb";
            SpecificName = "Activated";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?        
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            Icon = TextureManager.Get("bombset"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above?
            TintColor = Color.Firebrick; //what color should this powerup's icon and related effects be?
            Description = "Requires bomb charge!\nSets down a bomb that\ncan be detonated to\ndestroy walls and enemies"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            BombInterval = 1f;
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
            if (bombTimer >= BombInterval && (RetroGame.AvailableBombs > 0 || bombs.Count > 0))
            {
                if (bombs.Count == 0 && RetroGame.AvailableBombs > 0)
                {
                    bombs.Add(new Bomb(this, hero.position, "bomb", float.PositiveInfinity, ExplosionRadius));
                    bombTimer = 0;
                    RetroGame.RemoveBomb();
                }
                else
                {
                    bombs[0].detonate();
                    bombs.RemoveAt(0);
                    bombTimer = 0;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            base.Update(gameTime);
            if(!hero.Alive && bombs.Count > 0)
                bombs.Clear();
        }

        public override float GetPowerupCharge()
        {
            float charge = base.GetPowerupCharge();
            return charge;
        }
    }
}