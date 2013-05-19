using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public abstract class CoOpPowerup : Powerup
    {
        public Hero otherHero;

        public CoOpPowerup(Hero hero, Hero otherHero) :
            base(hero)
        {
            this.otherHero = otherHero;
        }
    }
}
