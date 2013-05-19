using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class DoNothingAI : AI
    {
        public void Reset()
        {
        }

        public void Update(GameTime gameTime)
        {
        }

        public Direction GetNextDirection(Entity subject)
        {
            return Direction.Up;
        }

        public float GetNextMoveSpeedMultiplier(Entity subject)
        {
            return 0;
        }
    }
}
