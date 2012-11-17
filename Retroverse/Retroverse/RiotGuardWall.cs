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
        public static string RIOTGUARD_TEXTURE_NAME = "riotguard1";
        public static Texture2D riotGuardTexture;

        private static readonly List<RiotGuard> guards = new List<RiotGuard>();
        public static readonly float INITIAL_WALL_POSITION = LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE * 0.9f;
        private static Vector2 wallPos;
        public static float wallPosition { get { return wallPos.X; } set { wallPos.X = value; } }

        public static readonly float[] WALL_SPEEDS = new float[] { 50, 90, 125, 150, 175, 200, 210, 220, 230, 240, 250, 300, 400, 600, 1000 };
        public static readonly float wallSpeedUpgradeTime = 80f; //seconds
        public static int wallSpeedIndex = 0;
        public static float wallSpeed = WALL_SPEEDS[0];
        public static float wallTime = 0;

        public static bool reversing = false;
        public static float timeToReverse;
        public static float reverseTime = 0;

        public static readonly int NUMBER_OF_GUARDS = 200;
        public static readonly int HORIZONTAL_POSITION_VARIATION = 15;
        public static readonly int VERTICAL_POSITION_VARIATION = 7;

        public static readonly int GUARD_HEIGHT_OFFSET_MIN = (int) (0.9f * -(NUMBER_OF_GUARDS / 2) * VERTICAL_POSITION_VARIATION);
        public static readonly int GUARD_HEIGHT_OFFSET_MAX = (int) (0.9f * (NUMBER_OF_GUARDS / 2 - 1) * VERTICAL_POSITION_VARIATION);

        public static void Initialize()
        {
            riotGuardTexture = TextureManager.Get(RIOTGUARD_TEXTURE_NAME);
            wallPos = new Vector2(INITIAL_WALL_POSITION, 0);
            int heroY = (int) Hero.instance.position.Y;
            guards.Clear();
            for (int i = -NUMBER_OF_GUARDS / 2; i < NUMBER_OF_GUARDS / 2; i++)
                guards.Add(new RiotGuard(new Vector2(Game1.rand.Next(HORIZONTAL_POSITION_VARIATION) - HORIZONTAL_POSITION_VARIATION / 3, heroY + i * VERTICAL_POSITION_VARIATION)));
        }

        public static void UpdateEscape(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            if (!reversing)
            {
                wallTime += seconds;
                if (wallTime >= wallSpeedUpgradeTime)
                {
                    if (wallSpeedIndex < WALL_SPEEDS.Length)
                        wallSpeed = WALL_SPEEDS[++wallSpeedIndex];
                    wallTime = 0;
                }
            }
            else
            {
                reverseTime += gameTime.getSeconds(1f);
                if (reverseTime >= timeToReverse)
                {
                    reversing = false;
                    reverseTime = 0;
                    wallSpeed *= -1;
                }
            }
            wallPosition += wallSpeed * seconds;

            updateGuards(gameTime);

            if (wallPosition >= Hero.instance.getLeft().X)
                Hero.instance.collideWithRiotGuardWall();
        }

        public static void reverse(float secondsToReverse)
        {
            reversing = true;
            timeToReverse = secondsToReverse;
            wallSpeed *= -1;
        }

        public static void UpdateRetro(GameTime gameTime)
        {
            updateGuards(gameTime);
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
                spriteBatch.Draw(riotGuardTexture, wallPos + position, null, Color.White, (float)Math.PI * 3 / 2, new Vector2(riotGuardTexture.Width / 2, riotGuardTexture.Height / 2), 0.6f, SpriteEffects.None, 0);
            }
        }
    }
}
