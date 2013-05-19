using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public abstract class ShieldPowerup : Powerup, IReversible
    {
        public Color ShieldColor { get; protected set; }
        public float MaxShieldRadius { get; protected set; }
        public float ShieldDeployRate { get; protected set; }
        public float ShieldDeployDelay { get; protected set; }

        public Texture2D shieldSprite;
        public Vector2 prevHeroPosition;
        public float timeSinceHeroStopped = 0f;
        public float shieldRadius = 0f;
        public float targetRadius = 0f;
        
        public const byte MIN_ALPHA = 100;
        public const byte MAX_ALPHA = 125;
        public byte currentAlpha = MIN_ALPHA;
        public const float ALPHA_ANIMATION_RATE = 2.5f;
        public float alphaAnimationRate = ALPHA_ANIMATION_RATE * (float)(1 - (RetroGame.rand.NextDouble() * 0.25));
        public float alphaAnimationAmount = (float)RetroGame.rand.NextDouble();
        public int alphaAnimationMultiplier = 1;
        public Color finalColor;

        public ShieldPowerup(Hero hero)
            : base(hero)
        {
            shieldSprite = TextureManager.Get("shield");
        }

        protected void InitializeSprites()
        {
            Texture2D shieldIconOriginal = TextureManager.Get("shieldicon");
            Color[] iconData = new Color[shieldIconOriginal.Width * shieldIconOriginal.Height];
            shieldIconOriginal.GetData<Color>(iconData);
            for (int i = 0; i < iconData.Length; i++)
            {
                iconData[i] = Color.FromNonPremultiplied(ShieldColor.R, ShieldColor.G, ShieldColor.B, iconData[i].A);
            }
            Texture2D shieldIcon = new Texture2D(shieldIconOriginal.GraphicsDevice, shieldIconOriginal.Width, shieldIconOriginal.Height);
            shieldIcon.SetData(iconData);
            Icon = shieldIcon;
        }

        public override void OnAddedToHero()
        {
            //Logic when added to hero here
        }

        public override void OnRemovedFromHero()
        {
            //Logic when removed from hero here
        }

        public override void Activate(InputAction activationAction)
        {
            //Activate logic here
        }

        // What does this shield do to the enemy?
        public abstract void AffectEnemy(Enemy e, float secondsPassed);

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);

            if (prevHeroPosition != hero.position)
                timeSinceHeroStopped = 0;
            else
                timeSinceHeroStopped += seconds;
            prevHeroPosition = hero.position;
            
            if (hero.Alive && timeSinceHeroStopped >= ShieldDeployDelay)
                targetRadius = MaxShieldRadius;
            else
                targetRadius = 0;

            if (targetRadius - shieldRadius > seconds * ShieldDeployRate)
                shieldRadius += ShieldDeployRate * seconds;
            else if (targetRadius - shieldRadius < -seconds * ShieldDeployRate)
                shieldRadius -= ShieldDeployRate * seconds;
            else
                shieldRadius = targetRadius;
            if (RetroGame.TopLevelManagerScreen.levelManager.LevelsSurroundingHero.ContainsKey(hero))
            {
                foreach (Level l in RetroGame.TopLevelManagerScreen.levelManager.LevelsSurroundingHero[hero])
                {
                    if (l != null)
                        foreach (Enemy e in l.enemies)
                        {
                            if (Vector2.Distance(hero.position, e.position) <= shieldRadius)
                                AffectEnemy(e, seconds);
                        }
                }
            }

            alphaAnimationAmount += alphaAnimationRate * alphaAnimationMultiplier * seconds;
            if (alphaAnimationAmount < 0 || alphaAnimationAmount >= 1)
            {
                alphaAnimationMultiplier *= -1;
                alphaAnimationAmount = MathHelper.Clamp(alphaAnimationAmount, 0, 1);
            }

            currentAlpha = (byte)MathHelper.SmoothStep(MIN_ALPHA, MAX_ALPHA, alphaAnimationAmount);
            finalColor = Color.FromNonPremultiplied(ShieldColor.withAlpha(currentAlpha).ToVector4());
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            charge = shieldRadius / MaxShieldRadius;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(shieldSprite, new Rectangle((int)(hero.position.X - shieldRadius), (int)(hero.position.Y - shieldRadius), (int)(shieldRadius * 2), (int)(shieldRadius * 2)), finalColor);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new ShieldMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class ShieldMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            Vector2 prevHeroPosition;
            float timeSinceHeroStopped;
            float shieldRadius = 0f;
            float targetRadius = 0f;
            float alphaAnimationAmount;
            int alphaAnimationMultiplier;

            public ShieldMemento(ShieldPowerup target)
            {
                //save necessary information from target here
                Target = target;
                prevHeroPosition = target.prevHeroPosition;
                timeSinceHeroStopped = target.timeSinceHeroStopped;
                shieldRadius = target.shieldRadius;
                targetRadius = target.targetRadius;
                alphaAnimationAmount = target.alphaAnimationAmount;
                alphaAnimationMultiplier = target.alphaAnimationMultiplier;
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                ShieldPowerup target = (ShieldPowerup)Target;
                if (nextFrame != null) //apply values with interpolation only if the next frame exists
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    ShieldMemento next = (ShieldMemento)nextFrame;
                    target.prevHeroPosition = prevHeroPosition * thisInterp + next.prevHeroPosition * nextInterp;
                    target.timeSinceHeroStopped = timeSinceHeroStopped * thisInterp + next.timeSinceHeroStopped * nextInterp;
                    target.shieldRadius = shieldRadius * thisInterp + next.shieldRadius * nextInterp;
                    target.alphaAnimationAmount = alphaAnimationAmount * thisInterp + next.alphaAnimationAmount * nextInterp;
                }
                else
                {
                    //do non-interpolative versions of the above applications here
                    target.prevHeroPosition = prevHeroPosition;
                    target.timeSinceHeroStopped = timeSinceHeroStopped;
                    target.shieldRadius = shieldRadius;
                    target.alphaAnimationAmount = alphaAnimationAmount;
                }
                //apply values that never need interpolation here
                target.targetRadius = targetRadius;
                target.alphaAnimationMultiplier = alphaAnimationMultiplier;
            }
        }
    }
}