using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class PatrolAI : AI
    {
        public static readonly Direction[] directions = new Direction[] { Direction.Left, Direction.None, Direction.Up, Direction.None, Direction.Right, Direction.None, Direction.Down, Direction.None };
        public static readonly float[] directionTimes = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        public static readonly int actionCount = directions.Length; 

        public int actionIndex = 0;
        public float actionTimer = 0;

        public void Reset()
        {
            actionIndex = 0;
            actionTimer = 0;
        }

        public void Update(GameTime gameTime)
        {
            actionTimer += gameTime.getSeconds();
            if (actionTimer >= directionTimes[actionIndex])
            {
                actionIndex = (actionIndex + 1) % actionCount;
                actionTimer = 0;
            }
        }

        public Direction GetNextDirection(Entity subject)
        {
            return directions[actionIndex];
        }

        public float GetNextMoveSpeedMultiplier(Entity subject)
        {
            return 1;
        }
    }
}
