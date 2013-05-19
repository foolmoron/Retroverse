using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public class Gem : Collectable
    {
        public Gem(int x, int y, int levelX, int levelY, int tileX, int tileY)
            : base(x, y, levelX, levelY, tileX, tileY)
        {
            CollectedSound = "CollectGem";
            this.setTexture("collectable3");
        }

        public override bool collectedBy(Entity e)
        {
            bool baseCollectedBy = base.collectedBy(e);
            if (baseCollectedBy)
            {
                RetroGame.AddGem();
                ((Hero)e).CollectedGems++;
            }
            return baseCollectedBy;
        }
    }
}
