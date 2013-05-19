using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class Sand : Collectable
    {
        public static readonly int SAND_SCORE = 100;
        public static readonly Color SAND_COLOR = new Color(140, 70, 20);

        public Sand(int x, int y, int levelX, int levelY, int tileX, int tileY)
            : base(x, y, levelX, levelY, tileX, tileY)
        {
            CollectedSound = "CollectSand";
            setTexture("sandicon");
            addsToProgress = true;
            scale = 0.5f;
            baseScore = SAND_SCORE;
            emitter.startColor = SAND_COLOR;
            emitter.endColor = new Color(SAND_COLOR.R, SAND_COLOR.G, SAND_COLOR.B, 0);
        }

        public override bool collectedBy(Entity e)
        {
            bool baseCollectedBy = base.collectedBy(e);
            if (baseCollectedBy)
            {
                RetroGame.AddSand();
                ((Hero)e).CollectedSand++;
            }
            return baseCollectedBy;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
