using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Particles;

namespace Retroverse
{
    public class LineEmitter : Emitter
    {
        public Vector2 positionB;

        public LineEmitter(Texture2D texture, Vector2 positionA, Vector2 positionB, float speed, ParticleDeathMode particleDeathMode, float valueToDeath, float startDistance, 
            float startSize, float endSize, Color startColor, Color endColor, ParticleRandType particleRandType, float startSizeDeviation, float endSizeDeviation, 
            float toDeathDeviation, float speedDeviation, float startColorDeviation, float endColorDeviation, int maxParticlesAtOnce, 
            float particlesPerSecond, int maxParticlesToEmit, bool snapToEmitter, bool reversible)
            : base(texture, positionA, speed, 0, particleDeathMode, valueToDeath, startDistance, 
                startSize, endSize, startColor, endColor, particleRandType, startSizeDeviation, endSizeDeviation, 
                toDeathDeviation, speedDeviation, startColorDeviation, endColorDeviation, 0, maxParticlesAtOnce, 
                particlesPerSecond, maxParticlesToEmit, snapToEmitter, reversible)
        {
            this.positionB = positionB;
        }
        
        /* Hard coded emitter temples to use... get the particle emitter using this function instead of building your own.*/
        public static LineEmitter getPrebuiltEmitter(PrebuiltLineEmitter prebuiltEmitter)
        #region Prebuilt Emitter Declarations
        {
            LineEmitter e = null;
            switch (prebuiltEmitter)
            {
                case PrebuiltLineEmitter.FlamethrowerFire:
                    e = new LineEmitter(TextureManager.Get("circle"),
                        Vector2.Zero,
                        Vector2.Zero,
                        100f,
                        ParticleDeathMode.Distance,
                        14f,
                        0,
                        0.2f,
                        0.7f,
                        new Color(255, 0, 0, 255),
                        new Color(255, 100, 0, 0),
                        ParticleRandType.Uniform,
                        0.1f,
                        0.3f,
                        0f,
                        0f,
                        0.25f,
                        0.50f,
                        1000,
                        1000,
                        -1,
                        false,
                        true
                        );
                    break;
                case PrebuiltLineEmitter.RiotGuardWallDrillSparks:
                    e = new LineEmitter(TextureManager.Get("circle"),
                        Vector2.Zero,
                        Vector2.Zero,
                        100,
                        ParticleDeathMode.Seconds,
                        0.5f,
                        0,
                        0.5f,
                        -1,
                        new Color(62, 62, 62, 255),
                        new Color(62, 62, 62, 128),
                        ParticleRandType.Uniform,
                        0.2f,
                        0.2f,
                        0,
                        50,
                        0,
                        0,
                        1000,
                        500,
                        -1,
                        false,
                        true
                        );
                    break;
                case PrebuiltLineEmitter.FireChainsFire:
                    e = new LineEmitter(TextureManager.Get("circle"),
                        Vector2.Zero,
                        Vector2.Zero,
                        30,
                        ParticleDeathMode.Distance,
                        12,
                        0,
                        0.3f,
                        0.1f,
                        new Color(255, 0, 0, 255), 
                        new Color(255, 160, 0, 50),
                        ParticleRandType.Uniform,
                        0.1f,
                        0.05f,
                        3f,
                        15f,
                        0.25f,
                        0,
                        1000,
                        300,
                        -1,
                        false,
                        true
                        );
                    break;
            }
            return e;
        }
        #endregion

        public override Particle newParticle(double particleOriginTime, bool createNewParticlesInReverse)
        {
            Vector2 orthogonalSlope = new Vector2(positionB.Y - position.Y, position.X - positionB.X); // get orthogonal line from opposite slope
            orthogonalSlope.Normalize();
            int directionFlip = 1;
            if (RetroGame.rand.Next(2) == 0)
                directionFlip = -1;
            Vector2 vUnit = orthogonalSlope * directionFlip;
            Vector2 velocity = vUnit * rand(speed, speedDeviation);
            float posInterp = rand(0.5f, 1f);
            Vector2 pPos = Vector2.Lerp(position, positionB, posInterp) + vUnit * startDistance;
            Color sColor = startColor;
            Color eColor = endColor;
            if (startColorDeviation > 0)
            {
                float r = rand(0, startColorDeviation);
                if (r > 0)
                {
                    sColor.R = (byte)(startColor.R * (1 - r) + endColor.R * r);
                    sColor.G = (byte)(startColor.G * (1 - r) + endColor.G * r);
                    sColor.B = (byte)(startColor.B * (1 - r) + endColor.B * r);
                    sColor.A = (byte)(startColor.A * (1 - r) + endColor.A * r);
                }
            }
            if (endColorDeviation > 0)
            {
                float r = rand(0, endColorDeviation);
                if (r < 0)
                {
                    sColor.R = (byte)(startColor.R * (-r) + endColor.R * (1 + r));
                    sColor.G = (byte)(startColor.G * (-r) + endColor.G * (1 + r));
                    sColor.B = (byte)(startColor.B * (-r) + endColor.B * (1 + r));
                    sColor.A = (byte)(startColor.A * (-r) + endColor.A * (1 + r));
                }
            }
            float pValueToDeath = rand(valueToDeath, toDeathDeviation);
            float pStartSize = rand(startSize, startSizeDeviation);
            float pEndSize = (endSize > 0) ? rand(endSize, endSizeDeviation) : pStartSize;
            Vector2 posShift = Vector2.Zero;
            if (createNewParticlesInReverse)
            {
                switch (deathMode)
                {
                    case ParticleDeathMode.Seconds:
                        particleOriginTime = particleOriginTime - valueToDeath;
                        break;
                    case ParticleDeathMode.Distance:
                        particleOriginTime = particleOriginTime - valueToDeath / velocity.Length();
                        break;
                }
            }
            return new Particle(
                    particleOriginTime,
                    texture,
                    pPos,
                    posShift,
                    vUnit,
                    velocity,
                    deathMode,
                    pValueToDeath,
                    pStartSize,
                    pEndSize,
                    sColor,
                    eColor
                );
        }

        public override IMemento GenerateMementoFromCurrentFrame()
        {
            return new LineEmitterMemento(this);
        }

        private class LineEmitterMemento : EmitterMemento
        {
            //add necessary fields to save information here
            Vector2 positionB;

            public LineEmitterMemento(LineEmitter target)
                : base(target)
            {
                positionB = target.positionB;
            }

            public override void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                base.Apply(interpolationFactor, isNewFrame, nextFrame);

                if (nextFrame != null)
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    LineEmitterMemento next = (LineEmitterMemento)nextFrame;
                    ((LineEmitter)Target).positionB = positionB * thisInterp + next.positionB * nextInterp;
                }
                else
                {
                    ((LineEmitter)Target).positionB = positionB;
                }
            }
        }
    }
}
