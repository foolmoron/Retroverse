using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public static class RiotGuardWall
    {
        public static readonly float SCALE_RIOT_GUARD = 0.6f;
        public static string RIOTGUARD_TEXTURE_NAME = "riotguard1";
        public static Texture2D riotGuardTexture;

        private static readonly List<RiotGuard> guards = new List<RiotGuard>();
        public static readonly float INITIAL_WALL_POSITION = LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE * 0.5f;
        private static Vector2 wallPos;
        public static float wallPosition { get { return wallPos.X; } set { wallPos.X = value; } }

        public const int WALL_SPEED_COUNT = 13;
        public static readonly float[] WALL_SPEEDS = new float[WALL_SPEED_COUNT]                  { 20, 45, 80, 100, 130, 170, 270, 400, 600, 800, 1200, 1600, 3000 };
        public static readonly float[] WALL_SPEED_UPGRADE_DISTANCES = new float[WALL_SPEED_COUNT] { 0,  7,  13, 17,  20,  23,  26,  28,  30,  31,  32,  33,  34}; //horizontal levels
        public static readonly float wallSpeedUpgradeTime = 80f; //seconds
        private static int wallSpeedIndex = 0;
        public static float wallSpeed = WALL_SPEEDS[0];
        public static float wallTime = 0;
        public static readonly float AUTO_GAMEOVER_DISTANCE = 40f;

        public static bool reversing = false;
        public static float timeToReverse;
        public static float reverseTime = 0;
        public static readonly float MAX_REVERSE_TIME = 5;

        public static readonly int NUMBER_OF_GUARDS = 200;
        public static readonly int HORIZONTAL_POSITION_VARIATION = 30;
        public static readonly int VERTICAL_POSITION_VARIATION = 7;

        public static readonly int GUARD_HEIGHT_OFFSET_MIN = (int) (0.9f * -(NUMBER_OF_GUARDS / 2) * VERTICAL_POSITION_VARIATION);
        public static readonly int GUARD_HEIGHT_OFFSET_MAX = (int) (0.9f * (NUMBER_OF_GUARDS / 2 - 1) * VERTICAL_POSITION_VARIATION);

        public static void Initialize(int checkpoint)
        {
            riotGuardTexture = TextureManager.Get(RIOTGUARD_TEXTURE_NAME);
            wallPos = new Vector2(INITIAL_WALL_POSITION, 0);
            int heroY = (int)Hero.instance.position.Y;
            wallSpeedIndex = 0;
            wallSpeed = WALL_SPEEDS[0];
            wallTime = 0;
            guards.Clear();
            for (int i = -NUMBER_OF_GUARDS / 2; i < NUMBER_OF_GUARDS / 2; i++)
                guards.Add(new RiotGuard(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + i * VERTICAL_POSITION_VARIATION)));
        }

        public static int getWallSpeedCount()
        {
            return 7;
        }

        public static int getCurrentWallSpeedIndex()
        {
            switch (wallSpeedIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    return wallSpeedIndex;
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    return 6;
                default:
                    return 0;
            }
        }

        public static void UpdateArena(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            resetGuardsIfNecessary(gameTime);
        }

        public static void UpdateEscape(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            if (!reversing)
            {
                wallTime += seconds;
                if (wallSpeedIndex < WALL_SPEED_COUNT - 1)
                {
                    if (Hero.instance.levelX >= WALL_SPEED_UPGRADE_DISTANCES[wallSpeedIndex + 1])
                    {
                        wallSpeed = WALL_SPEEDS[++wallSpeedIndex];
                        Game1.pulseVignette();
                    }
                }
                //if (wallTime >= wallSpeedUpgradeTime)
                //{
                //    if (wallSpeedIndex < WALL_SPEEDS.Length)
                //    {
                //        wallSpeed = WALL_SPEEDS[++wallSpeedIndex];
                //        Game1.pulseVignette();
                //    }
                //    wallTime = 0;
                //}
            }
            else
            {
                reverseTime += seconds;
                if (reverseTime >= MAX_REVERSE_TIME)
                {
                    setReverse(false);
                }
            }
            wallPosition += wallSpeed * seconds;

            updateGuards(gameTime);

            if (wallPosition - Hero.instance.position.X >= AUTO_GAMEOVER_DISTANCE)
            {
                Game1.gameOver();
            }

            if (wallPosition >= Hero.instance.getLeft().X)
                Hero.instance.collideWithRiotGuardWall();
        }

        public static void setReverse(bool reverse)
        {
            reversing = reverse;
            if (reverse && wallSpeed > 0)
                wallSpeed *= -1;
            else if (!reverse && wallSpeed < 0)
                wallSpeed *= -1;
        }

        public static void UpdateRetro(GameTime gameTime)
        {
            if (History.lastState == GameState.Escape)
            {
                wallPosition += wallSpeed * gameTime.getSeconds();
                updateGuards(gameTime);
            }
        }

        public static void updateGuards(GameTime gameTime)
        {
            float heroY = Hero.instance.position.Y;
            foreach (RiotGuard g in guards)
            {
                g.Update(gameTime);
                if (g.position.Y >= (heroY + GUARD_HEIGHT_OFFSET_MAX))
                    g.reset(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + GUARD_HEIGHT_OFFSET_MIN));
                else if (g.position.Y <= (heroY + GUARD_HEIGHT_OFFSET_MIN))
                    g.reset(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + GUARD_HEIGHT_OFFSET_MAX));
            }
        }

        public static void resetGuardsIfNecessary(GameTime gameTime)
        {
            float heroY = Hero.instance.position.Y;
            foreach (RiotGuard g in guards)
            {
                if (g.position.Y >= (heroY + GUARD_HEIGHT_OFFSET_MAX))
                    g.reset(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + GUARD_HEIGHT_OFFSET_MIN));
                else if (g.position.Y <= (heroY + GUARD_HEIGHT_OFFSET_MIN))
                    g.reset(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + GUARD_HEIGHT_OFFSET_MAX));
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (RiotGuard g in guards)
                g.Draw(spriteBatch);
        }

        public static void DrawDebug(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)wallPosition - 5, (int)Hero.instance.position.Y - 300, 10, 600), Color.HotPink);
        }

        private class RiotGuard
        {
            public static readonly float MOVE_SPEED = 50;
            public static readonly float RANDOM_MOVEMENT = 30;

            public Vector2 basePosition;
            public Vector2 startPosition;
            public Vector2 endPosition;
            public Vector2 position;
            public Vector2 unitDirection;
            public bool reachedEnd = true;

            public RiotGuard(Vector2 basePos)
            {
                basePosition = basePos;
                position = basePos;
            }

            public void reset(Vector2 basePos)
            {
                basePosition = basePos;
                position = basePos;
                reachedEnd = true;
            }

            public void Update(GameTime gameTime)
            {
                float seconds = gameTime.getSeconds();

                if (reachedEnd) // get new end position
                {
                    startPosition = position;
                    double randomAngle = Game1.rand.NextDouble() * Math.PI * 2;
                    float xMod = (float) Math.Cos(randomAngle);
                    float yMod = (float) Math.Sin(randomAngle);
                    endPosition = basePosition + new Vector2(xMod * RANDOM_MOVEMENT, yMod * RANDOM_MOVEMENT);
                    unitDirection = Vector2.Normalize(endPosition - startPosition);
                    reachedEnd = false;
                }

                position += unitDirection * MOVE_SPEED * seconds;
                Vector2 startToEnd = endPosition - startPosition;
                Vector2 posToEnd = endPosition - position;
                Vector2 distanceToEnd = posToEnd / startToEnd;
                if (distanceToEnd.X <= 0 || distanceToEnd.Y <= 0)
                    reachedEnd = true;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(riotGuardTexture, wallPos + position, null, Color.White, (float)Math.PI * 3 / 2, new Vector2(riotGuardTexture.Width / 2, riotGuardTexture.Height / 2), SCALE_RIOT_GUARD, SpriteEffects.None, 0);
            }
        }
    }
}
