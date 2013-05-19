using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class DrillFast : DrillPowerup, IReversible
    {
        public static readonly float DRILL_SINGLE_TIME = 1.25f; // seconds to drill

        public Emitter drillEmitter;
        protected string DrillSoundName { get; set; }

        public DrillFast(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Drill";
            SpecificName = "Fast";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("drillicon1"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.LightGray; //what color should this powerup's icon and related effects be?
            Description = "Drills through a\nsingle wall quickly"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            DrillSoundName = "FastDrillLoop";
            DrillTime = DRILL_SINGLE_TIME;
            drillEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
        }

        public override void Activate(InputAction activationAction)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);

            Direction direction = hero.direction;
            int levelX = hero.levelX, levelY = hero.levelY;
            int tileX = hero.tileX, tileY = hero.tileY;
            int nextTileX = hero.nextTileX, nextTileY = hero.nextTileY;
            Level nextLevel = hero.nextLevel;
            Vector2 position = hero.position;

            Vector2 drillOffset = Vector2.Zero;
            switch (direction)
            {
                case Direction.Up:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, -DRILL_OFFSET);
                    break;
                case Direction.Down:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, DRILL_OFFSET);
                    break;
                case Direction.Left:
                    drillOffset = new Vector2(-DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    break;
                case Direction.Right:
                    drillOffset = new Vector2(DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    break;
            }
            bool oldDrilling = drilling;
            drilling = false;
            if (nextLevel != null)
            {
                LevelContent.LevelTile nextTile = nextLevel.grid[nextTileX, nextTileY];
                if (!hero.moved && nextTile.Equals(LevelContent.LevelTile.Wall)) //drill
                {
                    drilling = true;
                    drillingTime += seconds;
                    if (drillingTime >= DrillTime)
                    {
                        nextLevel.drillWall(nextTileX, nextTileY);
                        drillingTime = 0;
                    }
                    drillingRatio = drillingTime / DrillTime;
                }
            }
            bool startedDrilling = !oldDrilling && drilling;
            bool stoppedDrilling = oldDrilling && !drilling;

            if (drilling)
            {
                drillEmitter.active = true;
                drillEmitter.position = position + drillOffset;
                drillEmitter.startSize = 1.5f * drillingRatio + 0.2f;
            }
            else
            {
                drillEmitter.active = false;
                drillingTime -= seconds * 3;
                if (drillingTime < 0)
                    drillingTime = 0;
                drillingRatio = drillingTime / DrillTime;
            }

            if (seconds > 0)
            {
                drillEmitter.Update(gameTime);
            }

            if (startedDrilling)
                SoundManager.PlaySoundOnLoop(DrillSoundName);
            else if (stoppedDrilling)
                SoundManager.StopLoopingSound(DrillSoundName);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            drillEmitter.Draw(spriteBatch);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new DrillFastMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class DrillFastMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float drillingTime;
            float drillingRatio;
            bool drilling;
            IMemento drillMemento;

            public DrillFastMemento(DrillFast target)
            {
                //save necessary information from target here
                Target = target;
                drillingTime = target.drillingTime;
                drillingRatio = target.drillingRatio;
                drilling = target.drilling;
                drillMemento = target.drillEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                DrillFast target = (DrillFast)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    DrillFastMemento next = (DrillFastMemento)nextFrame;
                    target.drillingTime = drillingTime * thisInterp + next.drillingTime * nextInterp;
                    target.drillingRatio = drillingRatio * thisInterp + next.drillingRatio * nextInterp;
                    drillMemento.Apply(interpolationFactor, isNewFrame, next.drillMemento);
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.drillingTime = drillingTime;
                    target.drillingRatio = drillingRatio;
                    drillMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                //apply values that never need interpolation here
                target.drilling = drilling;
            }
        }
    }
}