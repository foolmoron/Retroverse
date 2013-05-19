using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class ShieldSlow : ShieldPowerup
    {
        public const float SLOWING_FACTOR = 0.5f;

        public ShieldSlow(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Shield";
            SpecificName = "Slowing Shield";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.DeepSkyBlue; //what color should this powerup's icon and related effects be?
            Description = "Deploys a shield when\nnot moving that\nslows enemies in range";
            GemCost = COST_EXPENSIVE;
            
            ShieldColor = Color.DeepSkyBlue;
            MaxShieldRadius = 200f;
            ShieldDeployRate = 300f;
            ShieldDeployDelay = 0.5f;
            InitializeSprites();
        }

        public override void AffectEnemy(Enemy e, float secondsPassed)
        {
            e.globalMoveSpeedMultiplier -= SLOWING_FACTOR;
        }
    }
}