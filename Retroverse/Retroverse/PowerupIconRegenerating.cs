using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class PowerupIconRegenerating : PowerupIcon
    {
        public PowerupIconRegenerating(int x, int y, int levelX, int levelY, int tileX, int tileY, Type powerupType) :
            base(x, y, levelX, levelY, tileX, tileY, powerupType)
        {
            ActionAfterCollected = CollectedAction.Regenerate;
            emitter.valueToDeath = 1;
        }
    }
}
