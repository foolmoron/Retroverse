using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class Vignette
    {
        private static Texture2D tex;
        private static Color[] data;
        private static Point prevSize = new Point(0, 0);
        public static readonly double EXP_FACTOR = 1.1;
        public static readonly double FADE_FACTOR = 12;
        public static readonly double SHARPNESS_FACTOR = 0.4;
        private static readonly double MAX_CURVE_VALUE = Math.Pow(EXP_FACTOR, FADE_FACTOR);

        public static void Load()
        {
            int w = (int)RetroGame.screenSize.X;
            int h = (int)RetroGame.screenSize.Y - HUD.hudHeight;
            data = new Color[w * h];
            Vector2 center = new Vector2(w / 2, h / 2);
            float maxdistw = Vector2.DistanceSquared(center, new Vector2(0, center.Y));
            float maxdisth = Vector2.DistanceSquared(center, new Vector2(center.X, 0));
            for (int i = 0; i < data.Length; i++)
            {
                float distw = Vector2.DistanceSquared(center, new Vector2(i % w, 0));
                float disth = Vector2.DistanceSquared(center, new Vector2(0, i / w));
                float percent = Math.Max(distw / maxdistw, disth / maxdisth);
                percent = (float)((SHARPNESS_FACTOR * Math.Pow(EXP_FACTOR, FADE_FACTOR * percent) - 1) / MAX_CURVE_VALUE);
                percent = (percent >= 0.99f) ? 0.99f : percent;
                int a, r, b, g;
                r = 255 + (int)(510 - 255 * percent);
                g = 255 + (int)(510 - 255 * percent);
                b = 255 + (int)(510 - 255 * percent);
                a = (r >= g) ? r : g;
                a = (a >= b) ? a : b;
                a = 255 - a;
                //a = (a == 0) ? 25 : a;
                data[i].A = (a > 255) ? (byte)255 : (byte)a;
                data[i].R = (r > 255) ? (byte)255 : (byte)r;
                data[i].G = (g > 255) ? (byte)255 : (byte)g;
                data[i].B = (b > 255) ? (byte)255 : (byte)b;
            }
            prevSize = new Point(w, h);
        }

        public static void Draw(SpriteBatch spriteBatch, Color c, float intensity)
        {
            intensity = MathHelper.Clamp(intensity, 0, 1);
            int w = (int)RetroGame.screenSize.X;
            int h = (int)RetroGame.screenSize.Y - HUD.hudHeight;
            Point p = new Point(w, h);
            if (prevSize != p)
                Load();
            if (tex == null)
            {
                tex = new Texture2D(spriteBatch.GraphicsDevice, (int)RetroGame.screenSize.X, (int)RetroGame.screenSize.Y - HUD.hudHeight, false, SurfaceFormat.Color);
                tex.SetData(data);
            }

            spriteBatch.Draw(tex, new Vector2(0, HUD.hudHeight), null, c * intensity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f);
        }
    }
}
