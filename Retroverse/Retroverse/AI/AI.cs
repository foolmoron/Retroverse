using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public interface AI
    {
        // reset the AI
        void Reset();
        // update the AI every frame
        void Update(GameTime gameTime);
        // get the next movement direction
        Direction GetNextDirection(Entity subject);
        // get the next movement speed multiplier
        float GetNextMoveSpeedMultiplier(Entity subject);
    }
}
