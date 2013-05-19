using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public abstract class TargetedAI : AI
    {
        public Entity Target;

        public virtual void SetTarget(Entity target)
        {
            bool targetChanged = (this.Target != target);
            this.Target = target;
            if (targetChanged)
                OnTargetChanged();
        }

        public virtual void Reset()
        {
            Target = null;
        }

        public abstract void OnTargetChanged();
        public abstract void Update(GameTime gameTime);
        public abstract Direction GetNextDirection(Entity subject);
        public abstract float GetNextMoveSpeedMultiplier(Entity subject);
    }
}
