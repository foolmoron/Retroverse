using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public abstract class Pathfinding
    {
        public static readonly int COST_WALL = 100000;
        public static readonly int COST_FLOOR = 1;
        public static readonly int COST_MAX = 10000;

        public int gridWidth, gridHeight;

        protected struct OriginAndDestination
        {
            int originX;
            int originY;
            int destinationX;
            int destinationY;

            public OriginAndDestination(Point origin, Point destination)
            {
                originX = origin.X;
                originY = origin.Y;
                destinationX = destination.X;
                destinationY = destination.Y;
            }
        }
        protected Dictionary<OriginAndDestination, Direction> bestDirection;
        public int[,] costGrid;

        protected Pathfinding(int gridWidth, int gridHeight)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            costGrid = new int[gridWidth, gridHeight];
            bestDirection = new Dictionary<OriginAndDestination, Direction>();
        }

        public abstract void Reset();
        protected abstract double distance(Point origin, Point destination);
        public abstract Direction GetNextDirection(Point origin, Point destination);
    }
}
