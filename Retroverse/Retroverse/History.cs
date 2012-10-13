using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LevelPipeline;

namespace Retroverse
{
    public class History
    {
        private static Queue<Queue<History>> qq = new Queue<Queue<History>>();
        private static float secsSinceLastRetroPort = 0;
        private static HeroHistory retroHistory = null;
        private static Point lastLevel = new Point(-1, -1);

        private HeroHistory heroState;
        private LevelHistory levelState;

        private History() { }

        public static void Update(GameTime gameTime)
        {
            int x = Hero.instance.levelX;
            int y = Hero.instance.levelY;
            History h = new History();
            h.heroState = new HeroHistory(Hero.instance);
            h.levelState = new LevelHistory(Game1.levelManager.levels[x, y]);

            Point newPoint = new Point(x, y);
            if (lastLevel != newPoint)
                qq.Enqueue(new Queue<History>());
            lastLevel = newPoint;
            qq.Last().Enqueue(h);
            secsSinceLastRetroPort += (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (secsSinceLastRetroPort >= Game1.RETROPORT_SECS)
            {
                if (qq.First().Count == 0)
                    qq.Dequeue();
                h = qq.First().Dequeue();
                retroHistory = h.heroState;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (retroHistory != null)
                spriteBatch.Draw(retroHistory.tex, retroHistory.position, null, Color.White * 0.5f, retroHistory.rotation, new Vector2(retroHistory.tex.Width / 2, retroHistory.tex.Height / 2), 1f, SpriteEffects.None, 0.5f);
        }

        public static bool canRevert()
        {
            return retroHistory != null;
        }

        public static void revert()
        {
            retroHistory.apply();
            List<Point> alreadyReverted = new List<Point>();
            while (qq.Count > 0)
            {
                Queue<History> q = qq.Dequeue();
                LevelHistory lh = q.First().levelState;
                Point p = new Point(lh.x, lh.y);
                if (alreadyReverted.Contains(p))
                    continue;
                alreadyReverted.Add(p);
                lh.apply();
            }
            lastLevel = new Point(-1, -1);
            secsSinceLastRetroPort = 0;
            retroHistory = null;
        }

        private class HeroHistory
        {
            public Hero target;
            public Vector2 position;
            public Texture2D tex;
            public Direction dir;
            public float rotation;

            public HeroHistory(Hero h)
            {
                target = h;
                position = h.position;
                tex = h.getTexture();
                dir = h.direction;
                rotation = h.rotation;
            }

            public void apply()
            {
                target.position = position;
            }
        }

        public class LevelHistory
        {
            public Level target;
            public int x, y;
            public LevelContent.LevelTile[,] grid;
            public Color[] data = new Color[Level.TEX_SIZE * Level.TEX_SIZE];

            public LevelHistory(Level l)
            {
                target = l;
                x = l.xPos;
                y = l.yPos;
                grid = (LevelContent.LevelTile[,])l.grid.Clone();
                l.levelTexture.GetData<Color>(data);
            }

            public void apply()
            {
                target.grid = grid;
                target.levelTexture.SetData(data);
            }
        }
    }
}
