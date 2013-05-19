using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public abstract class BombPowerup : Powerup, IReversible
    {
        public const int BOMBS_ADDED_ON_COLLECT = 2;

        public float BombInterval { get; protected set; }
        public int ExplosionRadius { get; protected set; }
        public float bombTimer = 0;
        public List<Bomb> bombs;

        public BombPowerup(Hero hero)
            : base(hero)
        {
            bombs = new List<Bomb>();
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            bombTimer += seconds * hero.powerupCooldownModifier;

            for(int i = 0; i < bombs.Count; i++)
            {
                Bomb b = bombs[i];
                b.Update(gameTime);
                if(b.timeAlive >= b.timeToExplode)
                {
                    bombs.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void OnCollectedByHero(Hero collector)
        {
            for(int i = 0; i < BOMBS_ADDED_ON_COLLECT; i++)
            {
                RetroGame.AddBomb();
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = bombTimer / BombInterval;
            if(RetroGame.AvailableBombs <= 0)
                charge = 0;
            return charge;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            foreach (Bomb b in bombs)
            {
                b.Draw(spriteBatch);
            }
        }


        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new BombPowerupMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class BombPowerupMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            float bombTimer;
            List<Bomb> bombs;
            IMemento[] bombMementos;

            public BombPowerupMemento(BombPowerup target)
            {
                //save necessary information from target here
                Target = target;
                bombTimer = target.bombTimer;
                bombs = new List<Bomb>(target.bombs);
                bombMementos = new IMemento[target.bombs.Count];
                for (int i = 0; i < bombMementos.Length; i++)
                {
                    bombMementos[i] = target.bombs[i].GenerateMementoFromCurrentFrame();
                }
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                BombPowerup target = (BombPowerup)Target;
                if (isNewFrame)
                {
                    for (int i = 0; i < target.bombs.Count; i++)
                    {
                        if (!bombs.Contains(target.bombs[i]))
                        {
                            target.bombs.RemoveAt(i);
                            RetroGame.AddBomb();
                            i--;
                        }
                    }
                    foreach (Bomb b in bombs)
                    {
                        if (!target.bombs.Contains(b))
                            target.bombs.Add(b);
                    }
                }

                for (int i = 0; i < bombMementos.Length; i++)
                {
                    IMemento nextBombFrame = null;
                    if (nextFrame != null)
                    {
                        foreach (IMemento bm in ((BombPowerupMemento)nextFrame).bombMementos)
                        {
                            if (bm.Target == bombMementos[i].Target)
                            {
                                nextBombFrame = bm;
                                break;
                            }
                        }
                    }
                    bombMementos[i].Apply(interpolationFactor, isNewFrame, nextBombFrame);
                }

                if (nextFrame != null)
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    //cast the given memento to this specific type, don't worry about class cast exceptions
                    BombPowerupMemento next = (BombPowerupMemento)nextFrame;
                    target.bombTimer = bombTimer * thisInterp + next.bombTimer * nextInterp;
                }
                else
                {
                    target.bombTimer = bombTimer;
                }
            }
        }
    }
}