using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class DrillBasic : DrillFast
    {
        public static readonly float DRILL_BASIC_TIME = 2f; // seconds to drill

        public DrillBasic(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Drill";
            SpecificName = "Basic";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("normaldrill2"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = 0; //how many gems does it take to buy this from the store?
            TintColor = Color.Gray; //what color should this powerup's icon and related effects be?
            Description = "Drills through a\nsingle wall slowly"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            DrillSoundName = "BasicDrillLoop";
            DrillTime = DRILL_BASIC_TIME;
        }
    }
}