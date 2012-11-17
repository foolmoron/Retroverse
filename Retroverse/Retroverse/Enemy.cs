using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LevelPipeline;
using Particles;
using System.Diagnostics;

namespace Retroverse
{
    public class Enemy : Entity
    {
        public static readonly double CHANCE_TO_SPAWN_SAND_ON_DEATH = 0.20;
        public static readonly int ENEMY_KILL_SCORE = 250;
        public static readonly int WALL_COST = 100000;
        public static readonly int MAX_COST = 1000;
        public static readonly Point[] aimPoints = { new Point(0, 0), new Point(0, 2), new Point(2, 0), new Point(0, -2) };
        public Point aim, aimOffset;
        public Vector2 dirVector;
        public static readonly float MOVE_SPEED = Hero.MOVE_SPEED * 0.9f;
        public static LevelContent.LevelTile[,] original = new LevelContent.LevelTile[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public int[,] grid = new int[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public Point roundPosition;
        public int enemyUpdateFrequency = 10;
        HashSet<Point> walls = new HashSet<Point>();
        public int[] pqCorrection = new int[2000];
        private readonly Dictionary<Vector2, Direction> VECTOR_TO_DIR = new Dictionary<Vector2, Direction>(){
            {Vector2.Zero,Direction.None},
            {new Vector2(0, -1),Direction.Up},
            {new Vector2(0, 1),Direction.Down},
            {new Vector2(-1, 0),Direction.Left},
            {new Vector2(1, 0),Direction.Right},
        };
        private Level lvl;
        public static readonly int STARTING_HP = 5;
        public int hp;
        public int type;
        private int enemyUpdateCounter = 0;
        public bool dying = false;
        public Emitter emitter;
        public List<Bullet> bulletsAlreadyHit = new List<Bullet>();
        public static int idx = 0;
        public int id;

        public Enemy(int x, int y, int type, Level lv)
            : base(new Hitbox(32, 32))
        {
            lvl = lv;
            this.type = type;
            aimOffset = aimPoints[type];
            roundPosition = new Point(x, y);
            position = new Vector2(Level.TEX_SIZE * lvl.xPos + (x * Level.TILE_SIZE) + Level.TILE_SIZE / 2, Level.TEX_SIZE * lvl.yPos + (y * Level.TILE_SIZE) + Level.TILE_SIZE / 2);
            this.setTexture("enemy" + (type + 1));
            id = idx++;
            hp = 5;
            scale = 0.5f;

            //int levelX = (int)(position.X-x) / Level.TEX_SIZE; // get which level you are in
            //int levelY = (int)(position.Y-y) / Level.TEX_SIZE;
            original = lvl.grid;

            //grid=Game1.levelManager.levels[Game1.levelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y].grid;
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                    if (original[a, b] == LevelContent.LevelTile.Black)
                    {
                        grid[a, b] = WALL_COST;
                        walls.Add(new Point(a, b));
                    }
                    else grid[a, b] = 1;
            emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.EnemyDeathExplosion);
        }

        public Enemy(int x, int y, int type, Level l, Vector2 position, int hp, Direction dir, int texFrame, History.EmitterHistory eh, bool dying)
            : this(x, y, type, l)
        {
            this.position = position;
            this.hp = hp;
            this.direction = dir;
            setTextureFrame(texFrame);
            this.emitter = eh.emitter;
            this.emitter.particlesEmitted = eh.emitCount;
            this.emitter.active = eh.emitterActive;
            switch (direction)
            {
                case Direction.Up:
                    rotation = (float)Math.PI;
                    break;
                case Direction.Down:
                    rotation = 0;
                    break;
                case Direction.Left:
                    rotation = (float)Math.PI / 2f;
                    break;
                case Direction.Right:
                    rotation = (float)Math.PI * 3f / 2f;
                    break;
            }
            this.dying = dying;
        }

        public double dist(Point first, Point second)
        {
            //return Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y); //Manhattan distance
        }
        public Vector2 pathFinding(int pos)
        {
            enemyUpdateCounter++;
            enemyUpdateCounter %= enemyUpdateFrequency;
            if (enemyUpdateCounter != 0)
                return dirVector;
             
            //LevelContent.LevelTile[,] grid=LevelManager.levels[LevelManager.STARTING_LEVEL.X,LevelManager.STARTING_LEVEL.Y].grid;
            //pqCorrection = new int[2000];
            //aim = new Vector2(9,22);//
            aim = new Point(Hero.instance.tileX, Hero.instance.tileY);
            if (dist(roundPosition, aim) > 6)
            {
                Point newAim = new Point(aim.X + aimOffset.X, aim.Y + aimOffset.Y);
                if (!(newAim.X >= 31 || newAim.Y >= 31 || newAim.X < 0 || newAim.Y < 0) && !walls.Contains(newAim))
                    aim = newAim;
            }

            if (aim.X >= 31 || aim.Y >= 31 || aim.X < 0 || aim.Y < 0)
                return Vector2.Zero;
            if (aim.Equals(roundPosition))
            {
                //Console.WriteLine("ARRIVED");
                return Vector2.Zero;
                //return new Vector2(0, 0);
            }
            return lvl.aStar(roundPosition.X,roundPosition.Y,aim.X,aim.Y);
            //return lvl.cost[(int)roundPosition.X,(int)roundPosition.Y,(int)aim.X,(int)aim.Y];
            /*
            if (grid[(int)aim.X, (int)aim.Y] >= WALL_COST)
            {
                grid[(int)aim.X, (int)aim.Y] = 1;
                walls.Remove(aim);
            }
            SortedList<double, Point> queue = new SortedList<double, Point>(PREMADE_QUEUE);
            HashSet<Point> visited = new HashSet<Point>();
            Dictionary<Point, Point> path = new Dictionary<Point, Point>();
            int[,] best_cost = new int[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                {
                    best_cost[a, b] = MAX_COST;
                }
            best_cost[(int)roundPosition.X, (int)roundPosition.Y] = 0;

            queue.RemoveAt(queue.IndexOfValue(roundPosition));
            queue.Add(dist(roundPosition, aim), roundPosition);
            Point cur, neighbor;
            int score = MAX_COST;
            double cost_temp = 0;
            while (queue.Count > 0)
            {
                cur = queue.First().Value;
                if (cur == aim)
                {
                    if (best_cost[cur.X, cur.Y] >= MAX_COST)
                        return Vector2.Zero;
                    Point returnPoint = findPath(path, cur);
                    return new Vector2(returnPoint.X - roundPosition.X, returnPoint.Y - roundPosition.Y);
                }
                //return best_cost[(int)aim.X,(int)aim.Y];
                queue.RemoveAt(queue.IndexOfValue(cur));
                visited.Add(cur);
                for (int a = -1; a <= 1; a++)
                    for (int b = -1; b <= 1; b++)
                    {
                        if (cur.X + a < 0 || cur.Y + b < 0 || ((b != 0 && a != 0)) || cur.X + a > 30 || cur.Y + b > 30)
                            continue;
                        neighbor = new Point(cur.X + a, cur.Y + b);
                        if (visited.Contains(neighbor))
                            continue;
                        score = best_cost[(int)cur.X, (int)cur.Y] + grid[(int)cur.X + a, (int)cur.Y + b];
                        {
                            if ((!queue.ContainsValue(neighbor) || score < best_cost[(int)neighbor.X, (int)neighbor.Y])&&!path.ContainsKey(neighbor))
                            {
                                path.Add(neighbor, cur); 
                                if (queue.ContainsValue(neighbor))
                                    queue.RemoveAt(queue.IndexOfValue(neighbor));
                                best_cost[(int)neighbor.X, (int)neighbor.Y] = score;
                                cost_temp = best_cost[(int)neighbor.X, (int)neighbor.Y] + dist(neighbor, aim);

                                if (queue.IndexOfKey(cost_temp) == -1)
                                    queue.Add(cost_temp, neighbor);
                                else
                                    queue.Add(cost_temp + ((pqCorrection[(int)cost_temp]++) + 1) / (double)MAX_COST, neighbor);

                            }
                        }
                    }

            }
            //Console.WriteLine("FAILURE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            return new Vector2(roundPosition.X, roundPosition.Y);*/
        }
        public Point findPath(Dictionary<Point, Point> dic, Point node)
        {
            if (dic.ContainsKey(node) && !roundPosition.Equals(dic[node]))
                return findPath(dic, dic[node]);
            return node;
        }
        public Vector2 fourDirection(Vector2 dir)
        {
            if (dir == Vector2.Zero)
                return new Vector2(0, 0);
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                if (dir.X > 0)
                    return new Vector2(1, 0);
                else return new Vector2(-1, 0);
            if (dir.Y > 0)
                return new Vector2(0, 1);
            return new Vector2(0, -1);

        }
        public void Update(GameTime gameTime, bool doPathFinding)
        {
            if (dying)
            {
                emitter.position = position;
                emitter.Update(gameTime);
                if (emitter.isFinished())
                {
                    Game1.levelManager.enemiesToRemove.Add(this);
                }
            }
            else
            {
                if (doPathFinding)
                    dirVector = pathFinding(1);
                else 
                    dirVector = Vector2.Zero;
                //Console.WriteLine("normed: " + dirVector);
                dirVector = fourDirection(dirVector);
                //Console.WriteLine("direction: " + dirVector);
                //Console.WriteLine("Position: " + roundPosition);
                direction = VECTOR_TO_DIR[dirVector];

                float seconds = gameTime.getSeconds((Game1.retroStatisActive) ? Game1.timeScale / 3 : Game1.timeScale);

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
                        rotation = (float)Math.PI;
                        break;
                    case Direction.Down:
                        moved = canMove(new Vector2(0, movement.Y));
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        }
                        rotation = 0;
                        break;
                    case Direction.Left:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        rotation = (float)Math.PI / 2f;
                        break;
                    case Direction.Right:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        rotation = (float)Math.PI * 3f / 2f;
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

                //removes bullet when it hits an enemy
                foreach (Bullet b in Hero.instance.ammo)
                {
                    if (!bulletsAlreadyHit.Contains(b) && b.hitbox.intersects(hitbox))
                    {
                        b.collideWith(this);
                    }
                }


                position = new Vector2(nextX, nextY);
                roundPosition = new Point((int)((nextX - (Level.TEX_SIZE * lvl.xPos)) / Level.TILE_SIZE), (int)((nextY - (Level.TEX_SIZE * lvl.yPos)) / Level.TILE_SIZE));
            }
            base.Update(gameTime);
            if (!dying)
            {
                if (hitbox.intersects(Hero.instance.hitbox))
                {
                    Hero.instance.collideWithEnemy(this);
                }
            }
        }

        public void die()
        {
            Game1.addScore(ENEMY_KILL_SCORE);
            dying = true;
            if (Game1.rand.NextDouble() < CHANCE_TO_SPAWN_SAND_ON_DEATH)
            {
                int i = roundPosition.X;
                int j = roundPosition.Y;
                lvl.collectables[i, j] = new Sand(Level.TEX_SIZE * lvl.xPos + i * Level.TILE_SIZE + 16, Level.TEX_SIZE * lvl.yPos + j * Level.TILE_SIZE + 16, lvl.xPos, lvl.yPos, i, j);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (dying)
                emitter.Draw(spriteBatch);
            else
            {
                base.Draw(spriteBatch);
                hitbox.Draw(spriteBatch);
            }
        }
    }
}
