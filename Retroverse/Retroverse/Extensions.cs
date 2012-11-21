using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class Extensions
    {
        public static float getSeconds(this GameTime gameTime, float timeScale = -1f)
        {
            if (Game1.retroStatisActive)
            {
                if (timeScale < 0)
                    timeScale = Game1.timeScale;
                return (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
            } else
                return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static float getLuminosity(this Color color)
        {
            float luminosity = color.R * 0.30f + color.G * 0.59f + color.B * 0.11f;
            return luminosity;
        }

        public static Color withAlpha(this Color color, byte alpha)
        {
            color.A = alpha;
            return color;
        }
    }
}
