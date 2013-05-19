using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particles
{
    public class Particle
    {
        public double creationTimeAbsoluteSecs;
        public Texture2D texture;
        public Vector2 position;
        public Vector2 originalPos;
        public Vector2 posShift;
        public Vector2 vUnit;
        public Vector2 velocity;
        public Color color;
        public ParticleDeathMode deathMode;
        public float valueToDeath;
        public float time = 0;
        public float dist = 0;
        public float size = 1;
        public float startSize;
        public float endSize;
        public Color startColor;
        public Color endColor;
        public float lifecycle = 0;

        public Particle(double creationTimeAbsoluteSecs, Texture2D texture, Vector2 position, Vector2 posShift, Vector2 vUnit, Vector2 velocity, ParticleDeathMode deathMode,
            float valueToDeath, float startSize, float endSize, Color startColor, Color endColor)
        {
            this.creationTimeAbsoluteSecs = creationTimeAbsoluteSecs;
            this.texture = texture;
            this.originalPos = position;
            this.posShift = posShift;
            this.vUnit = vUnit;
            this.velocity = velocity;
            this.deathMode = deathMode;
            this.valueToDeath = valueToDeath;
            this.startSize = startSize;
            this.endSize = endSize;
            this.startColor = startColor;
            this.endColor = endColor;
            color = startColor;
        }

        public void Update(double currentTimeAbsoluteSecs, Emitter e)
        {
            time = (float)(currentTimeAbsoluteSecs - creationTimeAbsoluteSecs);
            posShift = time * velocity;
            dist = posShift.Length() * ((time < 0) ? -1 : 1);

            if (e == null)
                position = originalPos + posShift;
            else
                position = e.position + e.startDistance * vUnit + posShift;
            switch (deathMode)
            {
                case ParticleDeathMode.Seconds:
                    lifecycle = time / valueToDeath;
                    break;
                case ParticleDeathMode.Distance:
                    lifecycle = dist / valueToDeath;
                    break;
            }
            if (lifecycle > 1 || lifecycle < 0)
            {
                return;
            }
            float r, g, b, a;
            a = (startColor.A * (1 - lifecycle) + endColor.A * lifecycle);
            color.A = (byte) a;
            r = (startColor.R * (1 - lifecycle) + endColor.R * lifecycle);
            g = (startColor.G * (1 - lifecycle) + endColor.G * lifecycle);
            b = (startColor.B * (1 - lifecycle) + endColor.B * lifecycle);
            color.R = (byte)Math.Min(a, r);
            color.G = (byte)Math.Min(a, g);
            color.B = (byte)Math.Min(a, b);
            size = startSize * (1 - lifecycle) + endSize * lifecycle;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, color, 0f, new Vector2(texture.Width / 2, texture.Height / 2), size, SpriteEffects.None, 0);
        }
    }
}
