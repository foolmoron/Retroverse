using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class PlayerPrisoner : Prisoner
    {
        public Hero player;

        public PlayerPrisoner(Hero player, int levelX, int levelY, int tileX, int tileY)
            : base(player.color, player.prisonerName, levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2, levelX, levelY, tileX, tileY)
        {
            this.player = player;
            TAKEN_IDS[int.Parse(id)] = false;
            id = player.prisonerID.ToString("0000");
        }

        public override bool collectedBy(Entity e)
        {
            bool collected = base.collectedBy(e);
            if (collected)
            {
                player.revive(e.position);
            }
            return collected;
        }
    }
}
