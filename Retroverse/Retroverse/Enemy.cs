using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LevelPipeline;

namespace Retroverse
{
    public class Enemy : Entity
    {
        public static Enemy instance;
        public static Vector2[] aimPoints={new Vector2(2,1)};
        public Vector2 aim,dirVector,aimOffset;
        public static readonly float MOVE_SPEED = 200f;
        public static LevelContent.LevelTile[,] original = new LevelContent.LevelTile[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public int[,] grid = new int[LevelContent.LEVEL_SIZE,LevelContent.LEVEL_SIZE];
        public Vector2 roundPosition;
        public int[] pqCorrection = new int[2000];
        private readonly Dictionary<Vector2,Direction> VECTOR_TO_DIR = new Dictionary< Vector2,Direction>(){
            {Vector2.Zero,Direction.None},
            {new Vector2(0, -1),Direction.Up},
            {new Vector2(0, 1),Direction.Down},
            {new Vector2(-1, 0),Direction.Left},
            {new Vector2(1, 0),Direction.Right},
        };
        private Level lvl;

        public Enemy(int x, int y,int type,Level lv)
            : base(new Hitbox(32, 32))
        {
            lvl=lv;
            aimOffset = aimPoints[type];
            roundPosition = new Vector2(x, y);
            position = new Vector2(Level.TEX_SIZE * lvl.xPos + (x * Level.TILE_SIZE) + Level.TILE_SIZE/2, Level.TEX_SIZE * lvl.yPos + (y * Level.TILE_SIZE) + Level.TILE_SIZE/2);
            this.setTexture("enemy_test");
            instance = this;

            //int levelX = (int)(position.X-x) / Level.TEX_SIZE; // get which level you are in
            //int levelY = (int)(position.Y-y) / Level.TEX_SIZE;
            original = lvl.grid;
            
            //grid=Game1.levelManager.levels[Game1.levelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y].grid;
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                    if (original[a, b] == LevelContent.LevelTile.Black)
                        grid[a, b] = 100000;
                    else grid[a, b] = 1;
        }
        public double dist(Vector2 first, Vector2 second)
        {
            //return Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y); //Manhattan distance
        }
        public Vector2 pathFinding(int pos)
        {
            //LevelContent.LevelTile[,] grid=LevelManager.levels[LevelManager.STARTING_LEVEL.X,LevelManager.STARTING_LEVEL.Y].grid;
            pqCorrection = new int[2000];
            //aim = new Vector2(9,22);//
            aim = new Vector2((Hero.instance.position.X - Level.TEX_SIZE * lvl.xPos) / Level.TILE_SIZE,
                (Hero.instance.position.Y - Level.TEX_SIZE * lvl.yPos) / Level.TILE_SIZE);// +aimOffset;
            aim.X = (int)(aim.X);
            aim.Y = (int)(aim.Y);
            //Console.WriteLine("aim : "+aim);
            //aim = new Vector2(10, 13);
            if (aim.X >= 31 || aim.Y >= 31||aim.X<0||aim.Y<0)
                return Vector2.Zero;
            if (aim.Equals(roundPosition))
            {
                //Console.WriteLine("ARRIVED");
                return Vector2.Zero;
                //return new Vector2(0, 0);
            }
            SortedList<double,Vector2> queue=new SortedList<double,Vector2>();
            HashSet<Vector2> visited = new HashSet<Vector2>();
            Dictionary<Vector2, Vector2> path = new Dictionary<Vector2, Vector2>();
             int[,] best_cost=new int[LevelContent.LEVEL_SIZE,LevelContent.LEVEL_SIZE];
             for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++){
                    best_cost[a,b]=1000;
                    queue.Add(100+a*LevelContent.LEVEL_SIZE+b,new Vector2(a,b));
                }

           
            best_cost[(int)roundPosition.X, (int)roundPosition.Y] = 0;

            queue.RemoveAt(queue.IndexOfValue(roundPosition));
            queue.Add(dist(roundPosition,aim),roundPosition);
            Vector2 cur,neighbor;
            int score = 1000;
            double cost_temp=0;
             while (queue.Count > 0)
             {
                 cur = queue.First().Value;
                 if (cur == aim)
                     return findPath(path,cur)-roundPosition;
                     //return best_cost[(int)aim.X,(int)aim.Y];
                 queue.RemoveAt(queue.IndexOfValue(cur));
                 visited.Add(cur);
                 for (int a = -1; a <= 1; a++)
                     for (int b = -1; b <= 1; b++)
                     {
                         if (cur.X + a < 0 || cur.Y + b < 0||((b!=0&&a!=0))||cur.X+a>30||cur.Y+b>30)
                             continue;
                         neighbor=new Vector2(cur.X + a, cur.Y + b);
                         if (visited.Contains(neighbor))
                             continue;
                         score = best_cost[(int)cur.X, (int)cur.Y]+grid[(int)cur.X + a,(int)cur.Y + b];
                         if (!queue.ContainsValue(neighbor) || score < best_cost[(int)neighbor.X, (int)neighbor.Y])
                         {
                             path.Add(neighbor, cur); // error here when walking over a tile that used to be a wall
                             if (queue.ContainsValue(neighbor))
                                 queue.RemoveAt(queue.IndexOfValue(neighbor));
                             best_cost[(int)neighbor.X, (int)neighbor.Y] = score;
                             cost_temp=best_cost[(int)neighbor.X, (int)neighbor.Y]+dist(neighbor,aim);

                             if(queue.IndexOfKey(cost_temp)==-1)
                             queue.Add(cost_temp, neighbor);
                             else
                                 queue.Add(cost_temp+((pqCorrection[(int)cost_temp]++)+1)/1000.0, neighbor);

                         }
                     }
                 
             }
             //Console.WriteLine("FAILURE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
             return roundPosition;
        }
        public Vector2 findPath(Dictionary<Vector2, Vector2> dic, Vector2 node)
        {
            if (dic.ContainsKey(node)&&!roundPosition.Equals(dic[node]))
                return findPath(dic, dic[node]);
            return node;
        }
        public Vector2 fourDirection(Vector2 dir)
        {
            if (dir==Vector2.Zero)
                return new Vector2(0, 0);
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                if (dir.X > 0)
                    return new Vector2(1, 0);
                else return new Vector2(-1, 0);
            if (dir.Y > 0)
                return new Vector2(0, 1);
            return new Vector2(0, -1);

        }
        public override void Update(GameTime gameTime)
        {
            //Console.WriteLine("enemy is updating: "+pathFinding(1));

            dirVector = pathFinding(1);
            //Console.WriteLine("normed: " + dirVector);
            dirVector = fourDirection(dirVector);
            //Console.WriteLine("direction: " + dirVector);
            //Console.WriteLine("Position: " + roundPosition);
            direction = VECTOR_TO_DIR[dirVector];

            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Vector2 movement = Controller.dirVector * MOVE_SPEED * seconds;
            Vector2 movement = dirVector * (MOVE_SPEED) * seconds;
            float nextX = position.X + movement.X;
            float nextY = position.Y + movement.Y;
            bool moved = true;
            int n;
            switch (direction)
            {
                case Direction.Up:
                    moved = canMove(new Vector2(0, movement.Y));
                    if (!moved)
                    {
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                    }
                    rotation = 0;
                    break;
                case Direction.Down:
                    moved = canMove(new Vector2(0, movement.Y));
                    if (!moved)
                    {
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE/2 - (n % Level.TILE_SIZE);
                    }
                    rotation = (float)Math.PI;
                    break;
                case Direction.Left:
                    moved = canMove(new Vector2(movement.X, 0));
                    if (!moved)
                    {
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                    }
                    rotation = (float)Math.PI * 3f / 2f;
                    break;
                case Direction.Right:
                    moved = canMove(new Vector2(movement.X, 0));
                    if (!moved)
                    {
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                    }
                    rotation = (float)Math.PI / 2f;
                    break;
                default:
                    nextX = position.X;
                    nextY = position.Y;
                    break;
            }
            position = new Vector2(nextX, nextY);
            // check corners
            if (moved && 
                (Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) || //topleft
                Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) || //botleft
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) || //botright
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y)))) //topright
            {
                switch (direction)
                {
                    case Direction.Up: 
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    case Direction.Down:
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    case Direction.Left:
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        break;
                    case Direction.Right:
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        break;
                    default:
                        break;
                }
            }
            position = new Vector2(nextX, nextY);
            roundPosition = new Vector2((int)((nextX - (Level.TEX_SIZE * lvl.xPos)) / Level.TILE_SIZE), (int)((nextY - (Level.TEX_SIZE * lvl.yPos)) / Level.TILE_SIZE));
            
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            hitbox.Draw(spriteBatch);
        }
    }
}
    