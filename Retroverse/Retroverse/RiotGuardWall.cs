using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Particles;

namespace Retroverse
{
    public static class RiotGuardWall
    {
        public const float SCALE_RIOT_GUARD = 0.6f;
        public static string RIOTGUARD_TEXTURE_NAME = "riotguard1";
        public static Texture2D riotGuardTexture;

        public const float DRILLING_PARTICLE_BASE_SIZE = 0.2f;
        public const float DRILLING_PARTICLE_GROWTH = 0.75f;
        public static LineEmitter drillingEmitter;

        private enum RiotGuardMode { Waiting, Drilling, Moving }
        private static RiotGuardMode mode;
        private static RiotGuardOffset[] guardOffsets;
        public const int GUARD_OFFSET_COUNT = 30;
        private static Vector2 wallPos;
        public static float wallPosition { get { return wallPos.X; } set { wallPos.X = value; } }
        public const int WALL_WIDTH = 30;
        public static int levelX { get { return (int)((wallPos.X ) / Level.TEX_SIZE); } }
        public static int farthestLevelX;
        public const float MOVING_SPEED = 100f;
        public static bool IsWaiting { get { return mode == RiotGuardMode.Waiting; } }

        public static readonly Color ANGRY_COLOR = Color.Red;

        public const int LEVELS_TO_MAX_SPEED = 20;
        public const float MAX_TIME_TO_DRILL = 150f;
        public const float MIN_TIME_TO_DRILL = 30f;
        public static float timeToDrill;
        public static float drillingTime;

        public const float AUTO_GAMEOVER_DISTANCE = 40f;
        public static int oldHeroLevelX = LevelManager.STARTING_LEVEL.X;

        public static Rectangle[] heroCoverageArea;
        public static Rectangle guardCoverageArea;

        public const int HORIZONTAL_POSITION_VARIATION = 10;
        public const int VERTICAL_POSITION_VARIATION = 15;
        public const int GUARD_HEIGHT_OFFSET_MIN = -550;
        public const int GUARD_HEIGHT_OFFSET_MAX = 550;

        public static void Initialize(SaveGame saveGame = null)
        {
            riotGuardTexture = TextureManager.Get(RIOTGUARD_TEXTURE_NAME);
            drillingEmitter = LineEmitter.getPrebuiltEmitter(PrebuiltLineEmitter.RiotGuardWallDrillSparks);

            farthestLevelX = (saveGame != null) ? saveGame.levelX : LevelManager.STARTING_LEVEL.X;
            wallPos = new Vector2((farthestLevelX - 0.5f) * Level.TEX_SIZE, 0);
            if(saveGame != null) farthestLevelX++; //automatically chase after reloading
            int heroY = (int)RetroGame.getHeroes()[0].position.Y;
            mode = (saveGame != null) ? RiotGuardMode.Moving : RiotGuardMode.Waiting;
            drillingTime = 0;

            heroCoverageArea = new Rectangle[RetroGame.NUM_PLAYERS];

            guardOffsets = new RiotGuardOffset[GUARD_OFFSET_COUNT];
            for(int i = 0; i < GUARD_OFFSET_COUNT; i++)
            {
                guardOffsets[i] = new RiotGuardOffset();
            }
        }

        public static void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            drillingEmitter.active = false;
            int oldLevelX = levelX;
            switch (mode)
            {
                case RiotGuardMode.Waiting:
                    break;
                case RiotGuardMode.Drilling:
                    drillingTime += seconds;
                    if (drillingTime >= timeToDrill)
                        farthestLevelX++;

                    int heroLevelX = 0;
                    foreach (Hero hero in RetroGame.getHeroes())
                    {
                        if (hero.levelX > oldHeroLevelX)
                            heroLevelX = hero.levelX;
                    }
                    if(heroLevelX > farthestLevelX)
                        farthestLevelX = heroLevelX;

                    if (farthestLevelX > levelX)
                    {
                        StartMoving();
                    }

                    float drillingRatio = drillingTime / timeToDrill;
                    drillingEmitter.position = new Vector2(wallPosition + Level.TILE_SIZE / 2, guardCoverageArea.Top);
                    drillingEmitter.positionB = new Vector2(wallPosition + Level.TILE_SIZE / 2, guardCoverageArea.Bottom);
                    drillingEmitter.startSize = DRILLING_PARTICLE_GROWTH * drillingRatio + DRILLING_PARTICLE_BASE_SIZE;
                    drillingEmitter.active = true;
                    break;
                case RiotGuardMode.Moving:
                    wallPosition += MOVING_SPEED * seconds;
                    break;
            }
            drillingEmitter.Update(gameTime);
            updateGuards(gameTime);

            if(levelX > oldLevelX)
                mode = RiotGuardMode.Drilling;
            float levelInterp = 1 - (float)levelX / LEVELS_TO_MAX_SPEED;
            timeToDrill = levelInterp * (MAX_TIME_TO_DRILL - MIN_TIME_TO_DRILL) + MIN_TIME_TO_DRILL;

            foreach (Hero hero in RetroGame.getHeroes())
            {
                if(!hero.Alive)
                    continue;

                if (wallPosition - hero.position.X >= AUTO_GAMEOVER_DISTANCE)
                {
                    RetroGame.GameOver();
                }
                if (wallPosition >= hero.getLeft().X)
                    hero.collideWithRiotGuardWall();
            }
        }

        public static void StartMoving()
        {
            if (mode != RiotGuardMode.Moving)
                SoundManager.PlaySoundOnce("RiotGuardAlarm", playInReverseDuringReverse: true);
            drillingTime = 0;
            mode = RiotGuardMode.Moving;
        }

        public static void UpdateRetro(GameTime gameTime)
        {
            if (mode != RiotGuardMode.Waiting)
                updateGuards(gameTime);
        }

        private static void updateGuards(GameTime gameTime)
        {
            guardCoverageArea = Rectangle.Empty;
            for(int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                Hero hero = RetroGame.getHeroes()[i];
                heroCoverageArea[i].X = (int)(wallPosition - AUTO_GAMEOVER_DISTANCE);
                heroCoverageArea[i].Y = (int)(hero.position.Y + GUARD_HEIGHT_OFFSET_MIN);
                heroCoverageArea[i].Width = (int)AUTO_GAMEOVER_DISTANCE;
                heroCoverageArea[i].Height = GUARD_HEIGHT_OFFSET_MAX - GUARD_HEIGHT_OFFSET_MIN;
                if(guardCoverageArea == Rectangle.Empty)
                    guardCoverageArea = heroCoverageArea[i];
                else
                    guardCoverageArea = Rectangle.Union(guardCoverageArea, heroCoverageArea[i]);
            }

            foreach(RiotGuardOffset offset in guardOffsets)
            {
                offset.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Color color = Color.DimGray;
            switch (mode)
            {
                case RiotGuardMode.Waiting:
                    break;
                case RiotGuardMode.Drilling:
                    float interp = drillingTime / timeToDrill;
                    color = Color.Lerp(Color.White, ANGRY_COLOR, interp);
                    break;
                case RiotGuardMode.Moving:
                    color = ANGRY_COLOR;
                    break;
            }
            int baseY = guardCoverageArea.Top - (guardCoverageArea.Top % VERTICAL_POSITION_VARIATION);
            for (int xIndex = 0; xIndex < WALL_WIDTH / HORIZONTAL_POSITION_VARIATION; xIndex++)
            {
                int z = 0;
                for (int y = baseY; y < guardCoverageArea.Bottom; y += VERTICAL_POSITION_VARIATION)
                {
                    int offsetIndex = ((xIndex + 1) * (y / VERTICAL_POSITION_VARIATION + 1)) % GUARD_OFFSET_COUNT;
                    int x = guardCoverageArea.Left + (xIndex * HORIZONTAL_POSITION_VARIATION) - RiotGuardOffset.RANDOM_MOVEMENT + Level.TILE_SIZE / 2;
                    spriteBatch.Draw(riotGuardTexture, new Vector2(x, y) + guardOffsets[offsetIndex].offset, null, color, (float)Math.PI * 3 / 2, new Vector2(riotGuardTexture.Width / 2.0f, riotGuardTexture.Height / 2.0f), SCALE_RIOT_GUARD, SpriteEffects.None, 0);
                }
            }
            drillingEmitter.Draw(spriteBatch);
        }

        public static void DrawDebug(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)wallPosition - 5, (int)RetroGame.getHeroes()[0].position.Y - 300, 10, 600), Color.Lerp(Color.White, ANGRY_COLOR, drillingTime/timeToDrill).withAlpha(150));
//            spriteBatch.Draw(RetroGame.PIXEL, bounds[0], Color.Red.withAlpha(100));
//            spriteBatch.Draw(RetroGame.PIXEL, bounds[1], Color.Green.withAlpha(100));
//            spriteBatch.Draw(RetroGame.PIXEL, Rectangle.Union(bounds[0], bounds[1]), Color.Purple.withAlpha(100));
        }

        public static IMemento GenerateMementoFromCurrentFrame()
        {
            return new RiotGuardWallMemento();
        }

        private class RiotGuardWallMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            Vector2 wallPos;
            float drillingTime;
            float timeToDrill;
            RiotGuardMode mode;
            IMemento drillingEmitterMemento;

            public RiotGuardWallMemento()
            {
                wallPos = RiotGuardWall.wallPos;
                drillingTime = RiotGuardWall.drillingTime;
                timeToDrill = RiotGuardWall.timeToDrill;
                mode = RiotGuardWall.mode;
                drillingEmitterMemento = RiotGuardWall.drillingEmitter.GenerateMementoFromCurrentFrame();
            }
			
            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            { 			
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
					float thisInterp = 1 - interpolationFactor;
					float nextInterp = interpolationFactor;	
					//cast the given memento to this specific type, don't worry about class cast exceptions
                    RiotGuardWallMemento next = (RiotGuardWallMemento)nextFrame;
                    RiotGuardWall.wallPos = wallPos * thisInterp + next.wallPos * nextInterp;
                    RiotGuardWall.drillingTime = drillingTime * thisInterp + next.drillingTime * nextInterp;
                    drillingEmitterMemento.Apply(interpolationFactor, isNewFrame, next.drillingEmitterMemento);
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    RiotGuardWall.wallPos = wallPos;
                    RiotGuardWall.drillingTime = drillingTime;
                    drillingEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                //apply values that never need interpolation here
                RiotGuardWall.timeToDrill = timeToDrill;
                RiotGuardWall.mode = mode;
            }
        }

        private class RiotGuardOffset
        {
            public const int MOVE_SPEED = 50;
            public const int RANDOM_MOVEMENT = 30;

            public Vector2 startOffset;
            public Vector2 endOffset;
            public Vector2 offset;
            public Vector2 unitDirection;
            public bool reachedEnd = true;

            public RiotGuardOffset()
            {
                offset = Vector2.Zero;
            }

            public void Update(GameTime gameTime)
            {
                float seconds = gameTime.getSeconds();

                if (reachedEnd) // get new end offset
                {
                    startOffset = offset;
                    double randomAngle = RetroGame.rand.NextDouble() * Math.PI * 2;
                    float xMod = (float)Math.Cos(randomAngle);
                    float yMod = (float)Math.Sin(randomAngle);
                    endOffset = new Vector2(xMod * RANDOM_MOVEMENT, yMod * RANDOM_MOVEMENT);
                    unitDirection = Vector2.Normalize(endOffset - startOffset);
                    reachedEnd = false;
                }

                offset += unitDirection * MOVE_SPEED * seconds;
                Vector2 startToEnd = endOffset - startOffset;
                Vector2 posToEnd = endOffset - offset;
                Vector2 distanceToEnd = posToEnd / startToEnd;
                if (distanceToEnd.X <= 0 || distanceToEnd.Y <= 0)
                    reachedEnd = true;
            }
        }
    }
}
