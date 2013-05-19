using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Retroverse;

namespace Particles
{
    public class Emitter : IReversible
    {
        private double totalSeconds = 0;
        private static readonly Random random = new Random();
        public Texture2D texture;
        public Vector2 position;
        public float speed;
        public float angle;
        public ParticleDeathMode deathMode;
        public float valueToDeath;
        public float startDistance;
        public ParticleRandType randType;
        public float startSize;
        public float endSize;
        public Color startColor;
        public Color endColor;
        public float startSizeDeviation;
        public float endSizeDeviation;
        public float toDeathDeviation;
        public float speedDeviation;
        public float startColorDeviation;
        public float endColorDeviation;
        public float angleDeviation;
        public int maxParticlesToEmit;
        public int maxParticlesAtOnce;
        public float particlesPerSecond;
        public bool snapToEmitter;
        public bool reversible;

        public List<Particle> particles = new List<Particle>();
        public int particlesEmitted = 0;
        public int particlesDeadThisFrame = 0;
        public bool active = true;
        //public float targetAngle;
        //public float ANGULAR_VELOCITY = (float)Math.PI * 8;

        /* Hard coded emitter temples to use... get the particle emitter using this function instead of building your own.*/
        public static Emitter getPrebuiltEmitter(PrebuiltEmitter prebuiltEmitter)
        #region Prebuilt Emitter Declarations
        {
            Emitter e = null;
            switch (prebuiltEmitter)
            {
                case PrebuiltEmitter.BulletHitExplosion:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 100
                    , 0
                    , ParticleDeathMode.Distance
                    , 32
                    , 0
                    , 0.4f
                    , 0.4f
                    , new Color(255, 0, 0, 255)
                    , new Color(255, 0, 0, 0)
                    , ParticleRandType.Uniform
                    , 0.1f
                    , 0.05f
                    , 5
                    , 50
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 10
                    , -1
                    , 10
                    , false
                    , true
                    );
                    break;
                case PrebuiltEmitter.BurstBoostFire:
                    break;
                case PrebuiltEmitter.ChargingSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 40
                    , 0
                    , ParticleDeathMode.Seconds
                    , 0.3f
                    , 16
                    , 0.3f
                    , -1
                    , new Color(255, 215, 0, 255)
                    , new Color(255, 215, 0, 0)
                    , ParticleRandType.Uniform
                    , 0
                    , 0
                    , 0.1f
                    , 20
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 35
                    , 35
                    , -1
                    , false
                    , false
                    );
                    break;
                case PrebuiltEmitter.BlinkEndSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 40
                    , 0
                    , ParticleDeathMode.Seconds
                    , 0.3f
                    , 16
                    , 0.3f
                    , -1
                    , new Color(255, 215, 0, 255)
                    , new Color(255, 215, 0, 0)
                    , ParticleRandType.Uniform
                    , 0
                    , 0
                    , 0.1f
                    , 20
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 50
                    , 50
                    , 50
                    , false
                    , false
                    );
                    break;
                case PrebuiltEmitter.CollectedSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 75
                    , 0
                    , ParticleDeathMode.Seconds
                    , 1f
                    , 0
                    , 0.3f
                    , 0.7f
                    , new Color(111, 176, 172, 255)
                    , new Color(111, 176, 172, 0)
                    , ParticleRandType.Uniform
                    , 0.1f
                    , 0.2f
                    , 0
                    , 100
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 100
                    , -1
                    , 20
                    , false
                    , false
                    );
                    break;
                case PrebuiltEmitter.BlinkOriginSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 75
                    , 0
                    , ParticleDeathMode.Seconds
                    , 1f
                    , 0
                    , 0.3f
                    , 0.7f
                    , new Color(111, 176, 172, 255)
                    , new Color(111, 176, 172, 0)
                    , ParticleRandType.Uniform
                    , 0.1f
                    , 0.2f
                    , 0
                    , 100
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 100
                    , -1
                    , 20
                    , false
                    , false
                    );
                    break;
                case PrebuiltEmitter.DrillSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 100
                    , 0
                    , ParticleDeathMode.Seconds
                    , 0.5f
                    , 0
                    , 0.8f
                    , -1
                    , new Color(30, 30, 30, 128)
                    , new Color(30, 30, 30, 20)
                    , ParticleRandType.Uniform
                    , 0.2f
                    , 0.2f
                    , 0
                    , 50
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 100
                    , 20
                    , -1
                    , false
                    , true
                    );
                    break;
                case PrebuiltEmitter.EnemyDeathExplosion:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 100
                    , 0
                    , ParticleDeathMode.Distance
                    , 48
                    , 0
                    , 0.5f
                    , 0.5f
                    , new Color(71, 93, 65, 255)
                    , new Color(71, 93, 65, 0)
                    , ParticleRandType.Uniform
                    , 0
                    , 0
                    , 0
                    , 0
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 50
                    , -1
                    , 50
                    , false
                    , true
                    );
                    break;
                case PrebuiltEmitter.IdleBoostFire:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 30
                    , 0
                    , ParticleDeathMode.Distance
                    , 12
                    , 0
                    , 0.3f
                    , 0.1f
                    , Color.White
                    , new Color(0, 200, 100, 50)
                    , ParticleRandType.Gaussian
                    , 0.1f
                    , 0.05f
                    , 3
                    , 15
                    , 0.25f
                    , 0
                    , (float)(Math.PI / 2)
                    , 100
                    , 30
                    , -1
                    , true
                    , true
                    );
                    break;
                case PrebuiltEmitter.LargeBulletSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 80
                    , 0
                    , ParticleDeathMode.Seconds
                    , 0.75f
                    , 24
                    , 0.8f
                    , 0.3f
                    , new Color(0, 0, 0, 255)
                    , new Color(0, 0, 0, 0)
                    , ParticleRandType.Uniform
                    , 0.2f
                    , 0.1f
                    , 0.1f
                    , 75
                    , 0
                    , 0
                    , (float)(Math.PI)
                    , 100
                    , 70
                    , -1
                    , true
                    , true
                    );
                    break;
                case PrebuiltEmitter.MediumBulletSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 125
                    , 0
                    , ParticleDeathMode.Seconds
                    , 1f
                    , 0
                    , 0.6f
                    , 0.2f
                    , new Color(0, 0, 0, 255)
                    , new Color(0, 0, 0, 0)
                    , ParticleRandType.Uniform
                    , 0.2f
                    , 0.1f
                    , 0.2f
                    , 50
                    , 0
                    , 0
                    , (float)(Math.PI) / 3
                    , 75
                    , 40
                    , -1
                    , true
                    , true
                    );
                    break;
                case PrebuiltEmitter.PrisonerSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 75
                    , 0
                    , ParticleDeathMode.Seconds
                    , 3
                    , 0
                    , 0.5f
                    , 0.5f
                    , Color.White
                    , Color.White
                    , ParticleRandType.Uniform
                    , 0
                    , 0
                    , 0
                    , 0
                    , 0
                    , 0
                    , (float)(Math.PI * 2)
                    , 75
                    , -1
                    , 75
                    , false
                    , false
                    );
                    break;
                case PrebuiltEmitter.RocketBoostFire:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 60
                    , 0
                    , ParticleDeathMode.Distance
                    , 20
                    , 0
                    , 0.5f
                    , 0.1f
                    , new Color(255, 0, 0, 255)
                    , new Color(255, 160, 0, 50)
                    , ParticleRandType.Gaussian
                    , 0.2f
                    , 0.05f
                    , 5
                    , 15
                    , 0.25f
                    , 0
                    , (float)(Math.PI / 2)
                    , 100
                    , 50
                    , -1
                    , true
                    , true
                    );
                    break;
                case PrebuiltEmitter.SmallBulletSparks:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 100
                    , 0
                    , ParticleDeathMode.Seconds
                    , 1f
                    , 0
                    , 0.4f
                    , 0.2f
                    , new Color(0, 0, 0, 255)
                    , new Color(0, 0, 0, 0)
                    , ParticleRandType.Uniform
                    , 0.2f
                    , 0.1f
                    , 0.2f
                    , 50
                    , 0
                    , 0
                    , (float)(Math.PI / 8)
                    , 50
                    , 30
                    , -1
                    , true
                    , true
                    );
                    break;
                case PrebuiltEmitter.FlameFire:
                    e = new Emitter(TextureManager.Get("circle")
                    , new Vector2(0, 0)
                    , 40
                    , 0
                    , ParticleDeathMode.Distance
                    , 12
                    , 0
                    , 0.1f
                    , 0.9f
                    , new Color(255, 0, 0, 255)
                    , new Color(255, 70, 0, 0)
                    , ParticleRandType.Uniform
                    , 0.1f
                    , 0.2f
                    , 4f
                    , 0
                    , 0.25f
                    , 0.25f
                    , (float)(2 * Math.PI)
                    , 400
                    , 500
                    , -1
                    , true
                    , true
                    );
                    break;
            }
            return e;
        }
        #endregion

        public Emitter(Texture2D texture, Vector2 position, float speed, float angle, ParticleDeathMode particleDeathMode, float valueToDeath, float startDistance,
            float startSize, float endSize, Color startColor, Color endColor, ParticleRandType particleRandType, float startSizeDeviation, float endSizeDeviation, float toDeathDeviation,
            float speedDeviation, float startColorDeviation, float endColorDeviation, float angleDeviation, int maxParticlesAtOnce, float particlesPerSecond, int maxParticlesToEmit,
            bool snapToEmitter, bool reversible)
        {
            this.texture = texture;
            this.position = position;
            this.speed = speed;
            //this.targetAngle = angle;
            this.angle = angle;
            this.deathMode = particleDeathMode;
            this.valueToDeath = valueToDeath;
            this.startDistance = startDistance;
            this.randType = particleRandType;
            this.startSize = startSize;
            this.endSize = endSize;
            this.startColor = startColor;
            this.endColor = endColor;
            this.startSizeDeviation = startSizeDeviation;
            this.endSizeDeviation = endSizeDeviation;
            this.toDeathDeviation = toDeathDeviation;
            this.speedDeviation = speedDeviation;
            this.startColorDeviation = startColorDeviation;
            this.endColorDeviation = endColorDeviation;
            this.angleDeviation = angleDeviation;
            this.maxParticlesAtOnce = maxParticlesAtOnce;
            this.particlesPerSecond = particlesPerSecond;
            this.maxParticlesToEmit = maxParticlesToEmit;
            this.snapToEmitter = snapToEmitter;
            this.reversible = reversible;
        }

        public void Update(GameTime gameTime)
        {
            float timeModifier = 1f;
            float seconds = gameTime.getSeconds() * timeModifier;
            totalSeconds += seconds;
            particlesDeadThisFrame = 0;
            Update(totalSeconds, seconds, false);
        }

        protected void Update(double absoluteSeconds, float secondsPassed, bool createNewParticlesInReverse)
        {
            if (active || particlesDeadThisFrame > 0)
            {
                bool hasEmitLimit = maxParticlesToEmit > 0;
                if (!hasEmitLimit || particlesEmitted < maxParticlesToEmit || createNewParticlesInReverse)
                {
                    int particlesThisFrame = 0;
                    if (createNewParticlesInReverse)
                    {
                        particlesThisFrame = particlesDeadThisFrame;
                    }
                    else
                    {
                        particlesThisFrame = (particlesPerSecond > 0) ? (int)Math.Ceiling(secondsPassed * particlesPerSecond) : maxParticlesAtOnce;
                    }
                    for (int i = 0; i < particlesThisFrame && particles.Count < maxParticlesAtOnce && (!hasEmitLimit || particlesEmitted < maxParticlesToEmit || createNewParticlesInReverse); i++)
                    {
                        double particleOriginTime = absoluteSeconds;
                        particles.Add(newParticle(particleOriginTime, createNewParticlesInReverse));
                        if (!createNewParticlesInReverse)
                            particlesEmitted++;
                    }
                }
            }

            particlesDeadThisFrame = 0;
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Update(absoluteSeconds, (snapToEmitter) ? this : null);
                if (particles[i].lifecycle > 1 || particles[i].lifecycle < 0)
                {
                    particles.RemoveAt(i);
                    particlesDeadThisFrame++;
                    i--;
                }
            }

            if (isFinished())
            {
                if (particlesEmitted >= maxParticlesToEmit)
                    active = false;
            }
        }

        public virtual Particle newParticle(double particleOriginTime, bool createNewParticlesInReverse)
        {
            float a = rand(angle, angleDeviation);
            Vector2 vUnit = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
            Vector2 velocity = vUnit * rand(speed, speedDeviation);
            Vector2 pPos = position + vUnit * startDistance;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(spriteBatch);
            }
        }

        public bool isFinished()
        {
            return particles.Count == 0 && particlesEmitted > 0 && particlesDeadThisFrame == 0;
        }

        public void Reset()
        {
            particles.Clear();
            particlesEmitted = 0;
            active = false;
            totalSeconds = 0;
        }

        protected float rand(float mean, float deviation)
        {
            if (deviation == 0)
                return mean;
            switch (randType)
            {
                case ParticleRandType.Gaussian: 
                    double u1 = random.NextDouble();
                    double u2 = random.NextDouble();
                    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                    double randNormal = mean + deviation / 2 * randStdNormal;
                    if (randNormal > mean + deviation / 2 || randNormal < mean - deviation / 2)
                        return mean + deviation * ((float) random.NextDouble() - 0.5f);
                    else return (float)randNormal;
                case ParticleRandType.Uniform:
                    return mean + deviation * ((float)random.NextDouble() - 0.5f);
                case ParticleRandType.Constant:
                    return mean;
                default:
                    return mean;
            }
        }
        
        public virtual IMemento GenerateMementoFromCurrentFrame()
        {
            return new EmitterMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class EmitterMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            Vector2 position;
            float angle;
            float speed;
            float valueToDeath;
            float startSize;
            float endSize;
            Color startColor;
            Color endColor;

            bool active;
            double totalSeconds;
            int particlesEmitted;
            int particlesDeadThisFrame;

            public EmitterMemento(Emitter target)
            {
                //save necessary information from target here
                Target = target;
                totalSeconds = target.totalSeconds;
                position = target.position;
                particlesEmitted = target.particlesEmitted;
                particlesDeadThisFrame = target.particlesDeadThisFrame;
                angle = target.angle;
                active = target.active;
                speed = target.speed;
                valueToDeath = target.valueToDeath;
                startSize = target.startSize;
                endSize = target.endSize;
                startColor = target.startColor;
                endColor = target.endColor;
            }

            public virtual void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Emitter target = (Emitter)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    EmitterMemento next = (EmitterMemento)nextFrame;
                    target.totalSeconds = totalSeconds * thisInterp + next.totalSeconds * nextInterp;
                    target.position = position * thisInterp + next.position * nextInterp;
                    if (isNewFrame && particlesDeadThisFrame > 0)
                        target.particlesDeadThisFrame = particlesDeadThisFrame;
                    else
                        target.particlesDeadThisFrame = 0;
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.totalSeconds = totalSeconds;
                    target.position = position;
                }
                //apply values that never need interpolation here
                target.particlesEmitted = particlesEmitted;
                target.angle = angle;
                target.active = active;
                target.speed = speed;
                target.valueToDeath = valueToDeath;
                target.startSize = startSize;
                target.endSize = endSize;
                target.startColor = startColor;
                target.endColor = endColor;

                if (target.totalSeconds == 0)
                    target.particles.Clear();

               // if (target.active)
                    target.Update(target.totalSeconds, 0, true);
            }
        }
    }
}
