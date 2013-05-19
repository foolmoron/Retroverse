using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public abstract class DrillPowerup : Powerup
    {
        public const float DRILL_OFFSET = 16;

        public float DrillTime { get; protected set; }
        public float drillingTime = 0; // secs
        public float drillingRatio = 0; // secs
        public bool drilling = false;

        public Direction previousDir;

        protected DrillPowerup(Hero hero)
            : base(hero)
        {
            previousDir = hero.direction;
        }

        public override void Activate(InputAction activationAction)
        {            
        }

        public override float GetPowerupCharge()
        {
            float charge = 1;
            if (drillingRatio > 0)
                charge = drillingRatio;
            return charge;
        }

        public override void Update(GameTime gameTime)
        {
            if (hero.direction != previousDir) // reset drilling if direction changes
                drillingTime = 0;
            previousDir = hero.direction;
        }
    }
}