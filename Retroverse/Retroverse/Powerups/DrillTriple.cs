using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class DrillTriple : DrillPowerup, IReversible
    {
        public static readonly float DRILL_TRIPLE_TIME = 2f; // seconds to drill

        public Emitter drillEmitter;
        public Emitter drillEmitterLeft;
        public Emitter drillEmitterRight;

        public DrillTriple(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Drill";
            SpecificName = "Triple";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("drillicon2"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.DimGray; //what color should this powerup's icon and related effects be?
            Description = "Drills through three\nadjacent walls at once"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            DrillTime = DRILL_TRIPLE_TIME;
            drillEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
            drillEmitterLeft = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
            drillEmitterRight = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
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

            Vector2 drillOffset = Vector2.Zero, drillOffsetLeft = Vector2.Zero, drillOffsetRight = Vector2.Zero;
            bool drillingLeft = false;
            bool drillingRight = false;
            switch (direction)
            {
                case Direction.Up:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, -DRILL_OFFSET);
                    if (nextLevel != null)
                    {
                        if (nextTileX > 0)
                        {
                            drillingLeft = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X - Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX + 2, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X + 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                        if (nextTileX < Level.GRID_SIZE - 1)
                        {
                            drillingRight = nextLevel.grid[nextTileX + 1, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X + Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX - 2, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X - 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                    }
                    break;
                case Direction.Down:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, DRILL_OFFSET);
                    if (nextLevel != null)
                    {
                        if (nextTileX > 0)
                        {
                            drillingRight = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X - Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX + 2, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X + 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                        if (nextTileX < Level.GRID_SIZE - 1)
                        {
                            drillingLeft = nextLevel.grid[nextTileX + 1, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X + Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X - 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                    }
                    break;
                case Direction.Left:
                    drillOffset = new Vector2(-DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    if (nextLevel != null)
                    {
                        if (nextTileY > 0)
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY - 1] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y - Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY + 2] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y + 2 * Level.TILE_SIZE);
                        }
                        if (nextTileY < Level.GRID_SIZE - 1)
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY + 1] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y + Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY - 2] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y - 2 * Level.TILE_SIZE);
                        }
                    }
                    break;
                case Direction.Right:
                    drillOffset = new Vector2(DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    if (nextLevel != null)
                    {
                        if (nextTileY > 0)
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY - 1] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y - Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY + 2] == LevelContent.LevelTile.Wall;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y + 2 * Level.TILE_SIZE);
                        }
                        if (nextTileY < LevelContent.LEVEL_SIZE - 1)
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY + 1] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y + Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY - 2] == LevelContent.LevelTile.Wall;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y - 2 * Level.TILE_SIZE);
                        }
                    }
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
                        switch (direction)
                        {
                            case Direction.Up:
                            case Direction.Down:
                                if (!nextLevel.drillWall(nextTileX + 1, nextTileY))
                                    nextLevel.drillWall(nextTileX - 2, nextTileY);
                                if (!nextLevel.drillWall(nextTileX - 1, nextTileY))
                                    nextLevel.drillWall(nextTileX + 2, nextTileY);
                                break;
                            case Direction.Left:
                            case Direction.Right:
                                if (!nextLevel.drillWall(nextTileX, nextTileY + 1))
                                    nextLevel.drillWall(nextTileX, nextTileY - 2);
                                if (!nextLevel.drillWall(nextTileX, nextTileY - 1))
                                    nextLevel.drillWall(nextTileX, nextTileY + 2);
                                break;
                        }
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
                drillEmitterLeft.active = drillingLeft;
                drillEmitterLeft.position = position + drillOffsetLeft;
                drillEmitterLeft.startSize = 1.5f * drillingRatio + 0.2f;
                drillEmitterRight.active = drillingRight;
                drillEmitterRight.position = position + drillOffsetRight;
                drillEmitterRight.startSize = 1.5f * drillingRatio + 0.2f;
            }
            else
            {
                drillEmitter.active = false;
                drillingTime -= seconds * 3;
                if (drillingTime < 0)
                    drillingTime = 0;
                drillingRatio = drillingTime / DrillTime;
                drillEmitterLeft.active = false;
                drillEmitterRight.active = false;
            }

            if (seconds > 0)
            {
                drillEmitter.Update(gameTime);
                drillEmitterLeft.Update(gameTime);
                drillEmitterRight.Update(gameTime);
            }

            if (startedDrilling)
                SoundManager.PlaySoundOnLoop("TripleDrillLoop");
            else if (stoppedDrilling)
                SoundManager.StopLoopingSound("TripleDrillLoop");
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            drillEmitter.Draw(spriteBatch);
            drillEmitterLeft.Draw(spriteBatch);
            drillEmitterRight.Draw(spriteBatch);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new DrillTripleMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class DrillTripleMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float drillingTime;
            float drillingRatio;
            bool drilling;
            IMemento drillMemento;
            IMemento drillMementoLeft;
            IMemento drillMementoRight;

            public DrillTripleMemento(DrillTriple target)
            {
                //save necessary information from target here
                Target = target;
                drillingTime = target.drillingTime;
                drillingRatio = target.drillingRatio;
                drilling = target.drilling;
                drillMemento = target.drillEmitter.GenerateMementoFromCurrentFrame();
                drillMementoLeft = target.drillEmitterLeft.GenerateMementoFromCurrentFrame();
                drillMementoRight = target.drillEmitterRight.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                DrillTriple target = (DrillTriple)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    DrillTripleMemento next = (DrillTripleMemento)nextFrame;
                    target.drillingTime = drillingTime * thisInterp + next.drillingTime * nextInterp;
                    target.drillingRatio = drillingRatio * thisInterp + next.drillingRatio * nextInterp;
                    drillMemento.Apply(interpolationFactor, isNewFrame, next.drillMemento);
                    drillMementoLeft.Apply(interpolationFactor, isNewFrame, next.drillMementoLeft);
                    drillMementoRight.Apply(interpolationFactor, isNewFrame, next.drillMementoRight);
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.drillingTime = drillingTime;
                    target.drillingRatio = drillingRatio;
                    drillMemento.Apply(interpolationFactor, isNewFrame, null);
                    drillMementoLeft.Apply(interpolationFactor, isNewFrame, null);
                    drillMementoRight.Apply(interpolationFactor, isNewFrame, null);
                }
                //apply values that never need interpolation here
                target.drilling = drilling;
            }
        }
    }
}