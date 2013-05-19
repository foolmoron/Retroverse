using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class Bomb : Entity, IReversible
    {
        public const float MAX_ADDITIONAL_SCALE = 0.2f;

        public BombPowerup bombPowerup;
        public String textureName;
        public float timeToExplode;
        public float timeAlive = 0;
        public int explosionRadius;
        public float baseScale;

        public Bomb(BombPowerup bombPowerup, Vector2 position, String textureName, float timeToExplode, int explosionRadius)
            : base(position, new Hitbox(32, 32))
        {
            this.bombPowerup = bombPowerup;
            this.textureName = textureName;
            this.timeToExplode = timeToExplode;
            this.explosionRadius = explosionRadius;
            baseScale = scale;
            setTexture(textureName);
        }
        
        public void detonate()
        {
            SoundManager.PlaySoundOnce("BombExplosion", playInReverseDuringReverse: true);
            updateCurrentLevelAndTile();
            destroyTopWall(tileX, tileY);
            destroyBottomWall(tileX, tileY);
            destroyLeftWall(tileX, tileY);
            destroyRightWall(tileX, tileY);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            timeAlive += gameTime.getSeconds();
            if (!float.IsInfinity(timeToExplode))
            {
                float ratio = timeAlive / timeToExplode;
                if (ratio >= 1)
                    detonate();

                maskingColor = Color.Lerp(Color.White, Color.Red, ratio);
                scale = baseScale * (1 + (ratio * MAX_ADDITIONAL_SCALE));
            }
        }

        public void destroyLeftWall(int tX, int tY)
        {
            Level[,] levels = RetroGame.getLevels();
            Level level = levels[levelX, levelY];
            bool destroyed = false;
            int offsetX = 1;

            while (!destroyed && (offsetX <= explosionRadius))
            {
                int x = tX;
                int y = tY;
                x -= offsetX;
                try
                {
                    if (level.grid[x, y] == LevelContent.LevelTile.Wall)
                    {
                        level.drillWall(x, y);
                        if (offsetX == 1)
                        {
                            destroyTopWall(x, y);
                            destroyBottomWall(x, y);
                        }
                        else
                        {
                            destroyTopWall(x + 1, y);
                            destroyBottomWall(x + 1, y);
                        }
                        destroyed = true;
                    }
                    else
                    {
                        if (level.enemyGrid[x, y] != null)
                        {
                            Enemy e = level.enemyGrid[x, y];
                            e.dieFromHero(bombPowerup.hero);
                        }
                        destroyTopWall(x, y);
                        destroyBottomWall(x, y);
                        offsetX++;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    destroyed = true;
                }
            }
        }

        public void destroyRightWall(int tX, int tY)
        {
            Level[,] levels = RetroGame.getLevels();
            Level level = levels[levelX, levelY];
            bool destroyed = false;
            int offsetX = 1;

            while (!destroyed && (offsetX <= explosionRadius))
            {
                int x = tX;
                int y = tY;
                x += offsetX;
                try
                {
                    if (level.grid[x, y] == LevelContent.LevelTile.Wall)
                    {
                        level.drillWall(x, y);
                        if (offsetX == 1)
                        {
                            destroyTopWall(x, y);
                            destroyBottomWall(x, y);
                        }
                        else
                        {
                            destroyTopWall(x - 1, y);
                            destroyBottomWall(x - 1, y);
                        }
                        destroyed = true;
                    }
                    else
                    {
                        if (level.enemyGrid[x, y] != null)
                        {
                            Enemy e = level.enemyGrid[x, y];
                            e.dieFromHero(bombPowerup.hero);
                        }
                        destroyTopWall(x, y);
                        destroyBottomWall(x, y);
                        offsetX++;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    destroyed = true;
                }
            }
        }

        public void destroyTopWall(int tX, int tY)
        {
            Level[,] levels = RetroGame.getLevels();
            Level level = levels[levelX, levelY];
            bool destroyed = false;
            int offsetY = 1;
            while (!destroyed && (offsetY <= explosionRadius))
            {
                int x = tX;
                int y = tY;
                y -= offsetY;
                try
                {
                    if (level.grid[x, y] == LevelContent.LevelTile.Wall)
                    {
                        level.drillWall(x, y);
                        destroyed = true;
                    }
                    else
                    {
                        if (level.enemyGrid[x, y] != null)
                        {
                            Enemy e = level.enemyGrid[x, y];
                            e.dieFromHero(bombPowerup.hero);
                        }
                        offsetY++;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    destroyed = true;
                }
            }
        }

        public void destroyBottomWall(int tX, int tY)
        {
            Level[,] levels = RetroGame.getLevels();
            Level level = levels[levelX, levelY];
            bool destroyed = false;
            int offsetY = 1;
            while (!destroyed && (offsetY <= explosionRadius))
            {
                int x = tX;
                int y = tY;
                y += offsetY;
                try
                {
                    if (level.grid[x, y] == LevelContent.LevelTile.Wall)
                    {
                        level.drillWall(x, y);
                        destroyed = true;
                    }
                    else
                    {
                        if (level.enemyGrid[x, y] != null)
                        {
                            Enemy e = level.enemyGrid[x, y];
                            e.dieFromHero(bombPowerup.hero);
                        }
                        offsetY++;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    destroyed = true;
                }
            }
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new BombMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class BombMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float timeToExplode;
            float timeAlive;
            int explosionRadius;
            float scale;

            public BombMemento(Bomb target)
            {
                //save necessary information from target here
                Target = target;
                timeToExplode = target.timeToExplode;
                timeAlive = target.timeAlive;
                explosionRadius = target.explosionRadius;
                scale = target.scale;
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Bomb target = (Bomb)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    BombMemento next = (BombMemento)nextFrame;
                    target.timeAlive = timeAlive * thisInterp + next.timeAlive * nextInterp;
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.timeAlive = timeAlive;
                }
                //apply values that never need interpolation here
                target.timeToExplode = timeToExplode;
                target.explosionRadius = explosionRadius;
                target.scale = scale;
            }
        }
    }
}
