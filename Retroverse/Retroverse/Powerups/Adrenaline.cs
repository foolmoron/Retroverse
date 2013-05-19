using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class Adrenaline : Powerup
    {

        public float modifier; //How much faster the cooldowns are.

        public Adrenaline(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Adrenaline";
            SpecificName = "Permanent";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            GemCost = COST_VERYEXPENSIVE; //how many gems does it take to buy this from the store?
            Icon = TextureManager.Get("adrenalinepermanent");
            DrawBeforeHero = true;
            modifier = 1.25f;
            Description = "Permanently reduces\ncooldown times\nby 25%";
            TintColor = Color.Violet;
        }

        public override void OnAddedToHero()
        {
            //Nope
        }

        public override void OnRemovedFromHero()
        {
            hero.powerupCooldownModifier = 1f;
        }

        public override void Activate(InputAction activationAction)
        {
            //Not activated
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            
            hero.powerupCooldownModifier = modifier;
        }

        public override float GetPowerupCharge()
        {
            float charge = 1; //Doesn't charge
            return charge;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            //Doens't draw anything yet
        }
    }
}