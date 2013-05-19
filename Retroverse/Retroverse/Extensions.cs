using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class Extensions
    {
        private static readonly Dictionary<Direction, Vector2> DIR_TO_VECTOR = new Dictionary<Direction, Vector2>(){
            {Direction.None, Vector2.Zero},
            {Direction.Up, new Vector2(0, -1)},
            {Direction.Down, new Vector2(0, 1)},
            {Direction.Left, new Vector2(-1, 0)},
            {Direction.Right, new Vector2(1, 0)},
        };

        public static Vector2 toVector(this Direction dir)
        {
            return DIR_TO_VECTOR[dir];
        }

        public static double getAngleToHorizontal(this Vector2 vector)
        {
            return Math.PI - Math.Atan2(vector.Y, -vector.X);
        }

        public static Direction opposite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.None:
                    return Direction.None;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
            }
            return Direction.None;
        }

        public static Direction directionTo(this Point from, Point otherPoint)
        {
            Direction movement = Direction.None;
            if (otherPoint.Y > from.Y)
                movement = Direction.Down;
            else if (otherPoint.Y < from.Y)
                movement = Direction.Up;
            else if (otherPoint.X > from.X)
                movement = Direction.Right;
            else if (otherPoint.X < from.X)
                movement = Direction.Left;
            return movement;
        }

        public static float getSeconds(this GameTime gameTime, float timeScale = -1f)
        {
            if (RetroGame.retroStatisActive)
            {
                if (timeScale < 0)
                    timeScale = RetroGame.timeScale;
                return (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
            } else
                return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static Color Randomize(this Color color, byte minColor, byte alpha)
        {
            color.R = (byte)(RetroGame.rand.Next(255 - minColor) + minColor);
            color.G = (byte)(RetroGame.rand.Next(255 - minColor) + minColor);
            color.B = (byte)(RetroGame.rand.Next(255 - minColor) + minColor);
            color.A = alpha;
            return color;
        }

        public static float getLuminosity(this Color color)
        {
            float luminosity = color.R * 0.30f + color.G * 0.59f + color.B * 0.11f;
            return luminosity;
        }

        public static Color darkenIfTooLight(this Color color, float luminosityLimitPerc)
        {
            float luminosity = color.getLuminosity();
            if (luminosity > (luminosityLimitPerc * 255))
                return Color.Lerp(color, Color.Black, 1 - luminosityLimitPerc);
            else
                return color;
        }

        public static Color withAlpha(this Color color, byte alpha)
        {
            color.A = alpha;
            return Color.FromNonPremultiplied(color.ToVector4());
        }

        public static Color Tint(this Color original, Color color, float tintFactor = 1f)
        {
            Color tinted = Color.White;
            tinted.R = (byte)(original.R * ((color.R / 255f)));
            tinted.G = (byte)(original.G * ((color.G / 255f)));
            tinted.B = (byte)(original.B * ((color.B / 255f)));
            tinted.A = (byte)(original.A * ((color.A / 255f)));

            return Color.Lerp(original, tinted, tintFactor);
        }

        public static void setDataRectangle<T>(this T[,] destination, Rectangle rectangle, T[,] data)
        {
            for (int i = 0; i < rectangle.Width; i++)
                for (int j = 0; j < rectangle.Height; j++)
                {
                    destination[rectangle.Left + i, rectangle.Top + j] = data[i, j];
                }
        }
    }
}
