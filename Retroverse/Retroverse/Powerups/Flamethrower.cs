using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Particles;

namespace Retroverse
{
    public class Flamethrower : Powerup, IReversible
    {
        public List<Flame> flames;
        public Flame flame1;
        public Flame flame2;
        public Flame flame3;
        private bool active = false;
        private bool activatedThisFrame = false;
        private bool deactivatedThisFrame = false;

        public float flameDamagePerSecond = 4f;

        public Flamethrower(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Flamethrower";
            SpecificName = "Short";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = false; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("flamethrower"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = COST_VERYEXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.OrangeRed; //what color should this powerup's icon and related effects be?
            Description = "Burns enemies in\nfront of the hero"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            flames = new List<Flame>();
            flame1 = new Flame(hero, flameDamagePerSecond, 40);
            flame2 = new Flame(hero, flameDamagePerSecond, 70);
            flame3 = new Flame(hero, flameDamagePerSecond, 100);
            flames.AddRange(new[] {flame1, flame2, flame3});
        }

        public override void Activate(InputAction activationAction)
        {
            if (!active)
                activatedThisFrame = true;
            active = true;
        }

        public override float GetPowerupCharge()
        {
            float charge = 1;
            return charge;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Flame f in flames)
            {
                f.Update(gameTime, active);
            }
            if (!active)
                deactivatedThisFrame = true;
            active = false;

            if(activatedThisFrame)
                SoundManager.PlaySoundOnLoop("Flamethrower");
            else if(deactivatedThisFrame)
                SoundManager.StopLoopingSound("Flamethrower");
            activatedThisFrame = false;
            deactivatedThisFrame = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Flame flame in flames)
            {
                flame.Draw(spriteBatch);
               // if (RetroGame.DEBUG)
                    //flame.DrawDebug(spriteBatch);
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

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new FlamethrowerMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class FlamethrowerMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            public IMemento flame1Memento;
            public IMemento flame2Memento;
            public IMemento flame3Memento;

            public FlamethrowerMemento(Flamethrower target)
            {
                //save necessary information from target here
                Target = target;
                flame1Memento = target.flame1.flameEmitter.GenerateMementoFromCurrentFrame();
                flame2Memento = target.flame2.flameEmitter.GenerateMementoFromCurrentFrame();
                flame3Memento = target.flame3.flameEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Flamethrower target = (Flamethrower)Target;
                flame1Memento.Apply(interpolationFactor, isNewFrame, (nextFrame != null) ? ((FlamethrowerMemento)nextFrame).flame1Memento : null);
                flame2Memento.Apply(interpolationFactor, isNewFrame, (nextFrame != null) ? ((FlamethrowerMemento)nextFrame).flame2Memento : null);
                flame3Memento.Apply(interpolationFactor, isNewFrame, (nextFrame != null) ? ((FlamethrowerMemento)nextFrame).flame3Memento : null);
            }
        }
    }
}