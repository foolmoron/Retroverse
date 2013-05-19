using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Particles;

namespace Retroverse
{
    class RocketBoost : BoostPowerup
    {
        public const float BOOST_SPEED_MULTIPLIER = 1.25f;

        public RocketBoost(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Rocket";
            SpecificName = "Boost";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("boosticon2");
            DrawBeforeHero = true;
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.Orange; //what color should this powerup's icon and related effects be?
            Description = "Provides a steady\nboost to movement speed"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            leftBooster = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            rightBooster = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            moveSpeedMultiplier = BOOST_SPEED_MULTIPLIER;
        }

        public override void Activate(InputAction activationAction)
        {            
        }

        public override void Update(GameTime gameTime)
        {
            hero.globalMoveSpeedMultiplier *= moveSpeedMultiplier;
            leftBooster.active = hero.Alive;
            rightBooster.active = hero.Alive;
            base.Update(gameTime);
        }

        public override float GetPowerupCharge()
        {
            return 1f;
        }
    }
}
