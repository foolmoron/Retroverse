using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public abstract class Powerup : IComparable<Powerup>
    {
        public const int COST_CHEAP = 10;
        public const int COST_MEDIUM = 20;
        public const int COST_EXPENSIVE = 60;
        public const int COST_VERYEXPENSIVE = 150;

        public Hero hero;

        //Powerup properties - set these in the subclasses according to the powerup's specific needs
        public string GenericName { get; protected set; }
        public string SpecificName { get; protected set; }
        public int Rank { get; protected set; }
        public bool Active { get; protected set; }
        public bool StoreOnly { get; protected set; }
        public int GemCost { get; set; }
        public Texture2D Icon { get; protected set; }
        public bool DrawBeforeHero { get; protected set; }
        public Color TintColor { get; protected set; }
        private string description;
        public string Description
        {
            get { if (description == null) return "No description set for\npowerup " + GenericName + ":" + SpecificName; else return description; }
            protected set { description = value; }
        }

        public bool toRemove = false;

        protected Powerup(Hero hero)
        {
            this.hero = hero;
            TintColor = Color.White;
        }
        
        //Functions that are called when powerup is collected/added/removed from hero
        //Override them in the subclasses if you want
        public virtual void OnCollectedByHero(Hero collector) { }
        public virtual void OnAddedToHero() { }
        public virtual void OnRemovedFromHero() { }

        //Function that is called when the corresponding action is executed
        public abstract void Activate(InputAction activationAction);

        //Powerup logic - how it affects hero, etc.
        public abstract void Update(GameTime gameTime);

        //What percentage charged is the powerup
        public abstract float GetPowerupCharge();
        
        //Draw powerup effects
        public abstract void Draw(SpriteBatch spriteBatch);

        public int CompareTo(Powerup other)
        {
            return Rank - other.Rank;
        }
    }
}
