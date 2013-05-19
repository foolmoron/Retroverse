using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Retroverse
{
    public class Hitbox
    {
        public static bool DRAW_HITBOXES = true;
        public Vector2 offset;
        public Rectangle originalRectangle;
        public Rectangle rectangle;
        public bool rotateable;
        public bool active = true;
        public int top { get { return rectangle.Top; } private set{ } }
        public int left { get { return rectangle.Left; } private set { } }
        public int right { get { return rectangle.Right; } private set { } }
        public int bottom { get { return rectangle.Bottom; } private set { } }
        public int width { get { return rectangle.Width; } set { originalRectangle.Width = value; } }
        public int height { get { return rectangle.Height; } set { originalRectangle.Height = value; } }

        public Hitbox(int width, int height, bool rotateable = false)
        {
            this.offset = Vector2.Zero;
            this.rotateable = rotateable;
            originalRectangle = rectangle = new Rectangle(0, 0, width, height);
        }

        public Hitbox(Vector2 offset, int width, int height, bool rotateable = false)
        {
            this.offset = offset;
            this.rotateable = rotateable;
            originalRectangle = rectangle = new Rectangle(0, 0, width, height);
        }

        public override string ToString()
        {
            return rectangle.ToString();
        }


        public bool intersects(Hitbox otherHitbox)
        {
            if (!active || !otherHitbox.active)
                return false;
            return rectangle.Intersects(otherHitbox.rectangle);
        }

        public float intersectPercent(Hitbox otherHitbox)
        {
            if (!active || !otherHitbox.active)
                return 0f;
            Rectangle rout;
            Rectangle.Intersect(ref rectangle, ref otherHitbox.rectangle, out rout);
            return (float)(rout.Width * rout.Height) / (Math.Min(rectangle.Width * rectangle.Height, otherHitbox.rectangle.Width * otherHitbox.rectangle.Height));
        }

        public bool intersects(Point point)
        {
            if (!active)
                return false;
            return rectangle.Contains(point);
        }

        public bool intersectsLine(Vector2 point1, Vector2 point2, float thickness)
        {
            Vector2 a, b;
            if (point1.Y <= point2.Y)
            {
                a = point1;
                b = point2;
            }
            else
            {
                a = point2;
                b = point1;
            }
            float topMost = a.Y - thickness / 2;
            float bottomMost = b.Y + thickness / 2;
            float rightMost = Math.Max(a.X, b.X) + thickness / 2;
            float leftMost = Math.Min(a.X, b.X) - thickness / 2;

            float slope = (a.Y - b.Y) / (a.X - b.X);
            float intercept = a.Y - (slope * a.X);

            if (float.IsInfinity(slope))
            {
                float xLine = a.X;
                if (xLine >= rectangle.Left && xLine <= rectangle.Right &&
                    ((rectangle.Bottom >= topMost && rectangle.Bottom <= bottomMost) || (rectangle.Top >= topMost && rectangle.Top <= bottomMost)
                     || (rectangle.Bottom >= topMost && rectangle.Top <= bottomMost)))
                    return true;
                else
                    return false;
            }

            float xTop = (rectangle.Top - intercept) / slope;
            if (xTop >= rectangle.Left && xTop <= rectangle.Right && xTop >= leftMost && xTop <= rightMost && rectangle.Top <= bottomMost && rectangle.Top >= topMost)
                return true;
            float xBottom = (rectangle.Bottom - intercept) / slope;
            if (xBottom >= rectangle.Left && xBottom <= rectangle.Right && xBottom >= leftMost && xBottom <= rightMost && rectangle.Bottom <= bottomMost && rectangle.Bottom >= topMost)
                return true;
            float yRight = slope * rectangle.Right + intercept;
            if (yRight >= rectangle.Top && yRight <= rectangle.Bottom && yRight <= bottomMost && yRight >= topMost && rectangle.Right >= leftMost && rectangle.Right <= rightMost)
                return true;
            float yLeft = slope * rectangle.Left + intercept;
            if (yLeft >= rectangle.Top && yLeft <= rectangle.Bottom && yLeft <= bottomMost && yLeft >= topMost && rectangle.Left >= leftMost && rectangle.Left <= rightMost)
                return true;

            return false;
        }

        public void Update(Entity owner)
        {
            Vector2 newOffset = offset;
            int newWidth = originalRectangle.Width;
            int newHeight = originalRectangle.Height;
            if (rotateable)
            {
                switch (owner.direction)
                {
                    case Direction.Left:
                        newOffset.X = -offset.X;
                        break;
                    case Direction.Right:
                        break;
                    case Direction.Up:
                        newOffset.X = 0;
                        newOffset.Y = offset.X;
                        newWidth = originalRectangle.Height;
                        newHeight = originalRectangle.Width;
                        break;
                    case Direction.Down:
                        newOffset.X = 0;
                        newOffset.Y = -offset.X;
                        newWidth = originalRectangle.Height;
                        newHeight = originalRectangle.Width;
                        break;
                }
            }
            rectangle.Width = newWidth;
            rectangle.Height = newHeight;            
            int centerOffsetX = (right - left) / 2;
            int centerOffsetY = (bottom - top) / 2;
            rectangle.X = (int)(owner.position + newOffset).X - centerOffsetX;
            rectangle.Y = (int)(owner.position + newOffset).Y - centerOffsetY;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!DRAW_HITBOXES)
                return;
            int borderWidth = 2;
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(left, top, borderWidth, height), Color.LimeGreen); // Left
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(right, top, borderWidth, height), Color.LimeGreen); // Right
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(left, top, width, borderWidth), Color.LimeGreen); // Top
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(left, bottom, width, borderWidth), Color.LimeGreen); // Bottom
        }
    }
}
