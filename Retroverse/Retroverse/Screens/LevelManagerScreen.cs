using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public abstract class LevelManagerScreen : Screen
    {
        public LevelManager levelManager { get; protected set; }

        public Effect currentEffect = null;
        public bool drawEffects = false;

        public abstract void OnPaused();
    }
}
