using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class AStarPathfinding: Pathfinding
    {
        public AStarPathfinding(int gridWidth, int gridHeight)
            : base(gridWidth, gridHeight)
        {
        }

        public override void Reset()
        {
            bestDirection.Clear();
            costGrid = new int[gridWidth, gridHeight];
        }

        public override Direction GetNextDirection(Point origin, Point destination)
        {
            if (origin == destination)
                return Direction.None;
            OriginAndDestination oAndD = new OriginAndDestination(origin, destination);
            if (bestDirection.ContainsKey(oAndD))
                return bestDirection[oAndD];

            Point nextPoint = wikiAStar(origin, destination);            
            return origin.directionTo(nextPoint);
        }

        protected override double distance(Point origin, Point destination)
        {
            return Math.Abs(origin.X - destination.X) + Math.Abs(origin.Y - destination.Y); //Manhattan distance
        }

        public Point wikiAStar(Point origin, Point destination) //wikipedia pseudocode implementation of A*
        {
            //closedset := the empty set    // The set of nodes already evaluated.
            HashSet<Point> visited = new HashSet<Point>();
            //openset := {start}    // The set of tentative nodes to be evaluated, initially containing the start node
            SortedSet<AStarNode> openset = new SortedSet<AStarNode>();
            //came_from := the empty map    // The map of navigated nodes.
            Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
            //g_score[start] := 0    // Cost from start along best known path.
            Dictionary<Point, double> gScore = new Dictionary<Point, double>();
            gScore[origin] = 0;

            //// Estimated total cost from start to goal through y.
            //f_score[start] := g_score[start] + heuristic_cost_estimate(start, goal)
            openset.Add(new AStarNode(origin, gScore[origin] + distance(origin, destination)));

            //while openset is not empty
            while (openset.Count > 0)
            {
                //current := the node in openset having the lowest f_score[] value
                Point current = openset.Min.Point;
                //if current = goal
                if (current == destination)
                {
                    //return reconstruct_path(came_from, goal)
                    return constructPath(cameFrom, destination, destination);
                }

                //remove current from openset
                openset.Remove(openset.Min);
                //add current to closedset
                visited.Add(current);
                //for each neighbor in neighbor_nodes(current)
                foreach (Point neighbor in neighborPoints(current))
                {
                    //tentative_g_score := g_score[current] + dist_between(current,neighbor)
                    double nextGScore = gScore[current] + costGrid[neighbor.X, neighbor.Y];
                    //if neighbor in closedset
                    if (visited.Contains(neighbor))
                    {
                        //if tentative_g_score >= g_score[neighbor]
                        if (nextGScore >= gScore[neighbor])
                            //continue
                            continue;
                    }
                    //if neighbor not in openset or tentative_g_score < g_score[neighbor] 
                    AStarNode neighborNode = null;
                    foreach (AStarNode node in openset)
                        if (node.Point == neighbor)
                        {
                            neighborNode = node;
                            break;
                        }
                    if (neighborNode == null || nextGScore < gScore[neighbor])
                    {
                        //came_from[neighbor] := current
                        cameFrom[neighbor] = current;
                        //g_score[neighbor] := tentative_g_score
                        gScore[neighbor] = nextGScore;
                        //f_score[neighbor] := g_score[neighbor] + heuristic_cost_estimate(neighbor, goal)
                        double newFScore = gScore[neighbor] + distance(neighbor, destination);
                        if (neighborNode != null)
                        {
                            openset.Remove(neighborNode);
                        }
                        //if neighbor not in openset
                        //      add neighbor to openset
                        openset.Add(new AStarNode(neighbor, newFScore));
                    }
                }
            } 
            //return failure
            return origin;
        }

        public List<Point> neighborPoints(Point center)
        {
            List<Point> neighbors = new List<Point>(4);
            if (center.X < gridWidth - 1)
                neighbors.Add(new Point(center.X + 1, center.Y));
            if (center.X > 0)
                neighbors.Add(new Point(center.X - 1, center.Y));
            if (center.Y < gridHeight - 1)
                neighbors.Add(new Point(center.X, center.Y + 1));
            if (center.Y > 0)
                neighbors.Add(new Point(center.X, center.Y - 1));
            return neighbors;
        }

        public Point constructPath(Dictionary<Point, Point> cameFrom, Point current, Point goal) //wikipedia implementation
        {
            if (cameFrom.Count == 0)
                return current;
            //if came_from[current_node] in set
            Point next = cameFrom[current];
            bestDirection[new OriginAndDestination(next, goal)] = next.directionTo(current);
            if (cameFrom.ContainsKey(next))
            {
                //p := reconstruct_path(came_from, came_from[current_node])
                Point p = constructPath(cameFrom, next, goal);
                //return (p + current_node)
                return p;
            }
            //else
            else
            {
                //return current_node
                return current;
            }
        }

        private class AStarNode : IComparable
        {
            public Point Point { get; set; }
            public double fScore { get; set; }

            public AStarNode(Point p, double fScore)
            {
                this.Point = p;
                this.fScore = fScore;
            }

            public override string ToString()
            {
                return "" + fScore + ", " + Point;
            }

            public int CompareTo(object other)
            {
                if (other is AStarNode)
                {
                    int comp = (int)(this.fScore - ((AStarNode)other).fScore);
                    return (comp == 0) ? -1 : comp;
                }
                else return 1;
            }
        }
    }
}
