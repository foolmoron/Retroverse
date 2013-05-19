using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public abstract class GunPowerup : Powerup, IReversible
    {        
        public static readonly int BULLET_DAMAGE_NORMAL = 3;
        public static readonly float BULLET_NORMAL_SCALE = 0.375f;

        public string FiredSound { get; protected set; }
        public List<Bullet> ammo;
        public readonly float BULLET_FIRE_INTERVAL = 0.2f; //secs
        public float bulletTimer = 0;
        public bool activated = false;
        public bool shotFired = false;

        public float damageModifier = 1f;

        public List<Bullet> bulletsToRemove = new List<Bullet>();

        public int powerupGun; // 0=Normal, 1=Front, 2=Side, 3=Charge

        public GunPowerup(Hero hero)
            : base(hero)
        {
            ammo = new List<Bullet>();
        }

        public override void Activate(InputAction activationAction)
        {
            activated = true;
        }

        public override void Update(GameTime gameTime)
        {
            PreUpdate(gameTime);
            PostUpdate(gameTime);
        }

        public void PreUpdate(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            bulletTimer += seconds * hero.powerupCooldownModifier;

            damageModifier = 1f;

            if (bulletsToRemove != null)
                foreach (Bullet b in bulletsToRemove)
                {
                    ammo.Remove(b);
                }
            bulletsToRemove.Clear();
        }

        public void PostUpdate(GameTime gameTime)
        {
            foreach (Bullet b in ammo)
            {
                b.Update(gameTime);
            }

            activated = false;
            if (shotFired)
            {
                SoundManager.PlaySoundOnce(FiredSound, playInReverseDuringReverse: true);
                shotFired = false;
            }
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            foreach (Bullet b in ammo)
            {
                b.Draw(spriteBatch);
            }
        }

        public virtual IMemento GenerateMementoFromCurrentFrame()
        {
            return new GunPowerupMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        protected class GunPowerupMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            List<Bullet> ammo;
            IMemento[] bulletMementos;

            public GunPowerupMemento(GunPowerup target)
            {
                //save necessary information from target here
                Target = target;
                ammo = new List<Bullet>(target.ammo);
                bulletMementos = new IMemento[target.ammo.Count];
                for (int i = 0; i < bulletMementos.Length; i++)
                {
                    bulletMementos[i] = target.ammo[i].GenerateMementoFromCurrentFrame();
                }
            }

            public virtual void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                GunPowerup target = (GunPowerup)Target;
                if (isNewFrame)
                {
                    for (int i = 0; i < target.ammo.Count; i++)
			        {
                        if (!ammo.Contains(target.ammo[i]))
                        {
                            target.ammo.RemoveAt(i);
                            i--;
                        }
                    }
                    foreach (Bullet b in ammo)
                    {
                        if (!target.ammo.Contains(b))
                            target.ammo.Add(b);
                    }
                }

                for (int i = 0; i < bulletMementos.Length; i++)
                {
                    IMemento nextBulletFrame = null;
                    if (nextFrame != null)
                    {
                        foreach (IMemento bm in ((GunPowerupMemento)nextFrame).bulletMementos)
                        {
                            if (bm.Target == bulletMementos[i].Target)
                            {
                                nextBulletFrame = bm;
                                break;
                            }
                        }
                    }
                    bulletMementos[i].Apply(interpolationFactor, isNewFrame, nextBulletFrame);
                }
            }
        }
    }
}