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
            spriteBatch.Draw(Game1.PIXEL, new Rectangle(left, top, borderWidth, height), Color.LimeGreen); // Left
            spriteBatch.Draw(Game1.PIXEL, new Rectangle(right, top, borderWidth, height), Color.LimeGreen); // Right
            spriteBatch.Draw(Game1.PIXEL, new Rectangle(left, top, width, borderWidth), Color.LimeGreen); // Top
            spriteBatch.Draw(Game1.PIXEL, new Rectangle(left, bottom, width, borderWidth), Color.LimeGreen); // Bottom
        }
    }
}
