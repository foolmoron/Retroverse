using System.Collections.Generic;
using System.Linq;
using LevelPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Retroverse
{
    public class History
    {
        private static Queue<Queue<History>> qq = new Queue<Queue<History>>();
        private static float secsSinceLastRetroPort = 0;
        private static History retroHistory = null;
        private static Point lastLevel = new Point(-1, -1);

        // retroport values
        public static readonly float RETROPORT_SECS = 3f;
        public static bool cancel = false;

        // retroport reverting effect values
        public static List<int> queueDict = new List<int>();
        public static int[] queueIndices;
        public static int retroportFrames = 0;
        public static readonly float FRAME_VELOCITY_MIN = 10;
        public static readonly float FRAME_VELOCITY_MAX = 45;
        public static float frameVelocity = FRAME_VELOCITY_MIN; //historical frames per second to rewind
        public static float frame = 0;
        public static int prevFrame = -1;
        public static float secsInRetroPort = 0;
        public static int frameAccelerationModifier = 1;

        // retroport intro/outro effect values
        public static Boolean effectFinished = true;
        public static float EFFECT_FINISHED_RADIUS;
        public static readonly float effectIntroVelocity = 900f;
        public static readonly float effectOutroVelocity = 900f;

        // grayscale effect values
        public static float EFFECT_RADIUS_MAX;
        public static float EFFECT_RADIUS_MIN = 16;
        public static float effectRadius = float.PositiveInfinity;
        public static float effectPerc;
        public static float effectIntensity = 2f;
        public static readonly Func<float, float> effectFunc = (perc) =>
        {
            if (perc <= 0.5f)
                return -2 * perc + 1;
            else
                return 2 * perc - 1;
        };
        public static readonly float EFFECT_FUNC_MAX = 0.0625f;

        private HeroHistory heroState;
        private LevelHistory levelState;
        private EnemyHistory enemyState;

        private History() { }

        public static void UpdateArena(GameTime gameTime)
        {
        }

        public static void UpdateEscape(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            int x = Hero.instance.levelX;
            int y = Hero.instance.levelY;
            History h = new History();
            h.heroState = new HeroHistory(Hero.instance);
            h.levelState = new LevelHistory(Game1.levelManager.levels[x, y]);
            h.enemyState = new EnemyHistory(Game1.levelManager.levels[x, y]);

            Point newPoint = new Point(x, y);
            if (lastLevel != newPoint)
                qq.Enqueue(new Queue<History>());
            lastLevel = newPoint;
            qq.Last().Enqueue(h);
            secsSinceLastRetroPort += (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (secsSinceLastRetroPort >= RETROPORT_SECS)
            {
                if (qq.First().Count == 0)
                    qq.Dequeue();
                h = qq.First().Dequeue();
                retroHistory = h;
            }

            EFFECT_FINISHED_RADIUS = Game1.screenSize.X;
            if (effectRadius < EFFECT_FINISHED_RADIUS)
            {
                Game1.drawEffects = true;
                Game1.currentEffect = Effects.OuterGrayscale;
                effectRadius += effectOutroVelocity * seconds;
                Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
                Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
            }
            else
                effectFinished = true;

            if (effectFinished)
                effectRadius = Game1.screenSize.Y / 1.5f;
        }

        public static void UpdateRetro(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            if (effectRadius > EFFECT_RADIUS_MAX + EFFECT_RADIUS_MIN)
            {
                Game1.drawEffects = true;
                Game1.currentEffect = Effects.OuterGrayscale;
                effectRadius -= effectIntroVelocity * seconds;
                if (effectRadius < EFFECT_RADIUS_MAX + EFFECT_RADIUS_MIN)
                {
                    effectRadius = EFFECT_RADIUS_MAX + EFFECT_RADIUS_MIN;
                    effectFinished = false;
                }
                Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
                Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                return;
            }

            if (retroportFrames == 0)
            {
                int i = 0;
                int j = 0;
                queueIndices = new int[qq.Count];
                foreach (Queue<History> q in qq)
                {
                    queueIndices[j] = retroportFrames;
                    retroportFrames += q.Count;
                    while (i < retroportFrames)
                    {
                        queueDict.Add(j);
                        i++;
                    }
                    j++;
                }
            }

            float perc = frame / retroportFrames;
            float perc2 = 2 * perc;
            if (perc2 < 1)
                frameVelocity = FRAME_VELOCITY_MIN * (1 - perc2) + FRAME_VELOCITY_MAX * perc2;
            else
            {
                perc2 -= 1;
                frameVelocity = FRAME_VELOCITY_MIN * perc2 + FRAME_VELOCITY_MAX * (1 - perc2);
            }
            if (frame > retroportFrames / 2 && frameAccelerationModifier > 0)
                frameAccelerationModifier = -1;
            frame += frameVelocity * seconds;

            Game1.drawEffects = true;
            Game1.currentEffect = Effects.OuterGrayscale;
            effectRadius = EFFECT_RADIUS_MAX * effectFunc(perc) + EFFECT_RADIUS_MIN;
            Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
            Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
            Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
            Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);

            int iframe = retroportFrames - (int)frame - 1;
            float interpolation = frame - (int)frame;
            if (iframe >= 0)
            {
                int queue = queueDict[iframe];
                History currentFrame = qq.ElementAt(queue).ElementAt(iframe - queueIndices[queue]);
                History nextFrame = null;
                if (iframe > 0)
                {
                    int queue2 = queueDict[iframe - 1];
                    nextFrame = qq.ElementAt(queue2).ElementAt((iframe - 1) - queueIndices[queue2]);
                }
                currentFrame.heroState.apply(interpolation, nextFrame);
                currentFrame.levelState.apply(interpolation, nextFrame);
                currentFrame.enemyState.apply(interpolation, nextFrame);
                prevFrame = iframe;
            }
            if (iframe < 0 || cancel)
            {
                qq.Clear();
                queueDict.Clear();
                queueIndices = null;
                retroportFrames = 0;
                frame = 0;
                prevFrame = -1;
                secsInRetroPort = 0;
                frameAccelerationModifier = 1;
                frameVelocity = FRAME_VELOCITY_MIN;
                lastLevel = new Point(-1, -1);
                secsSinceLastRetroPort = 0;
                retroHistory = null;
                cancel = false;
                Game1.levelManager.scrollMultiplier = 3f;
                Game1.state = GameState.Escape;
                Game1.drawEffects = false;
            }
        }

        public static void DrawHero(SpriteBatch spriteBatch)
        {
            if (retroHistory != null)
            {
                HeroHistory hHero = retroHistory.heroState;
                spriteBatch.Draw(hHero.tex, hHero.position, null, Color.White * 0.5f, hHero.rotation, new Vector2(hHero.tex.Width / 2, hHero.tex.Height / 2), 1f, SpriteEffects.None, 0);
            }
        }

        public static void DrawLevel(SpriteBatch spriteBatch, int xPos, int yPos)
        {
            if (retroHistory != null)
            {
                foreach (Queue<History> q in qq)
                {
                    if (q.Count == 0)
                        continue;
                    LevelHistory lh = q.First().levelState;
                    if (xPos == lh.x && yPos == lh.y)
                    {
                        spriteBatch.Draw(lh.getTexture(), new Vector2(lh.x * Level.TEX_SIZE, lh.y * Level.TEX_SIZE), null, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
                        break;
                    }
                }
            }
        }

        public static void DrawEnemies(SpriteBatch spriteBatch)
        {
            if (retroHistory != null)
            {
                List<Point> alreadyDrawn = new List<Point>();
                foreach (Queue<History> q in qq)
                {
                    if (q.Count == 0)
                        continue;
                    EnemyHistory hEnemies = q.First().enemyState;
                    Point p = new Point(hEnemies.x, hEnemies.y);
                    if (alreadyDrawn.Contains(p))
                        continue;
                    alreadyDrawn.Add(p);
                    foreach (IndividualEnemyHistory hEnemy in hEnemies.histories)
                    {
                        spriteBatch.Draw(hEnemy.tex, hEnemy.position, null, Color.White * 0.5f, hEnemy.rotation, new Vector2(hEnemy.tex.Width / 2, hEnemy.tex.Height / 2), 1f, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public static void setEffectRadiusMax()
        {
            EFFECT_RADIUS_MAX = Math.Max(Game1.screenSize.X, Game1.screenSize.Y) / 6;
        }

        public static bool canRevert()
        {
            return retroHistory != null;
        }

        public static void cancelRevert()
        {
            if (!effectFinished)
                cancel = true;
        }

        private class HeroHistory
        {
            public Hero target;
            public Vector2 position;
            public Texture2D tex;
            public int texFrame;
            public float rotation;

            public HeroHistory(Hero h)
            {
                target = h;
                position = h.position;
                tex = h.getTexture();
                texFrame = h.getTextureFrame();
                rotation = h.rotation;
            }

            public void apply()
            {
                target.position = position; ;
                target.rotation = rotation;
            }

            public void apply(float interp, History nextFrame)
            {
                if (nextFrame == null)
                    apply();
                else
                {
                    float thisInterp = 1 - interp;
                    target.position = position * thisInterp + nextFrame.heroState.position * interp;
                    target.rotation = rotation;
                }
            }
        }

        private class LevelHistory
        {
            public Level target;
            public int x, y;
            public LevelContent.LevelTile[,] grid;
            public Texture2D tex;

            public LevelHistory(Level l)
            {
                target = l;
                x = l.xPos;
                y = l.yPos;
                grid = (LevelContent.LevelTile[,])l.grid.Clone();
            }

            public void apply()
            {
                target.grid = grid;
                target.levelTexture = getTexture();
            }

            public void apply(float interp, History nextFrame)
            {
                if (nextFrame == null)
                    apply();
                else
                {
                    target.grid = grid;
                    target.levelTexture = getTexture();
                }
            }

            public Texture2D getTexture()
            {
                if (tex == null)
                {
                    tex = new Texture2D(Game1.graphicsDevice, Level.TEX_SIZE, Level.TEX_SIZE);
                    Color[] tiledata = new Color[Level.TILE_SIZE * Level.TILE_SIZE];
                    for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                        for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                        {
                            if (Level.TILE_TO_TEXTURE[grid[i, j]] == null)
                                continue;
                            Level.TILE_TO_TEXTURE[grid[i, j]].GetData<Color>(tiledata);
                            tex.SetData<Color>(0, new Rectangle(i * Level.TILE_SIZE, j * Level.TILE_SIZE, Level.TILE_SIZE, Level.TILE_SIZE), tiledata, 0, Level.TILE_SIZE * Level.TILE_SIZE);
                        }
                }
                return tex;
            }
        }

        private class EnemyHistory
        {
            public Level target;
            public int x, y;
            public IndividualEnemyHistory[] histories;

            public EnemyHistory(Level l)
            {
                target = l;
                x = l.xPos;
                y = l.yPos;
                histories = new IndividualEnemyHistory[l.enemies.Count];
                int i = 0;
                foreach (Enemy e in l.enemies)
                {
                    IndividualEnemyHistory h = new IndividualEnemyHistory(e, e.position, e.getTexture(), e.rotation, e.getTextureFrame(), e.direction);
                    histories[i++] = h;
                }
            }

            public void apply()
            {
                foreach (IndividualEnemyHistory h in histories)
                {
                    Enemy e = h.target;
                    e.position = h.position;
                    e.setTextureFrame(h.texFrame);
                    e.direction = h.dir;
                }
            }

            public void apply(float interp, History nextFrame)
            {
                if (nextFrame == null)
                    apply();
                else
                {
                    float thisInterp = 1 - interp;
                    if (nextFrame.enemyState.x != x || nextFrame.enemyState.y != y)
                    {
                        apply();
                    }
                    else
                    {
                        for (int i = 0; i < histories.Length; i++)
                        {
                            IndividualEnemyHistory h1 = histories[i];
                            IndividualEnemyHistory h2 = nextFrame.enemyState.histories[i];
                            Enemy e = h1.target;
                            e.position = h1.position * thisInterp + h2.position * interp;
                            e.setTextureFrame(h1.texFrame);
                            e.direction = h1.dir;
                        }
                    }
                }
            }
        }

        private struct IndividualEnemyHistory
        {
            public Enemy target;
            public Vector2 position;
            public Texture2D tex;
            public float rotation;
            public int texFrame;
            public Direction dir;

            public IndividualEnemyHistory(Enemy e, Vector2 pos, Texture2D t, float rot, int f, Direction d)
            {
                target = e;
                position = pos;
                tex = t;
                rotation = rot;
                texFrame = f;
                dir = d;
            }
        }
    }
}
