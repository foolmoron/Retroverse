using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;
using System.Diagnostics;

namespace Retroverse
{
    public class Enemy : Entity
    {
        public static readonly double CHANCE_TO_SPAWN_SAND_ON_DEATH = 0.05;
        public static readonly int ENEMY_KILL_SCORE = 150;
        public static readonly int WALL_COST = 100000;
        public static readonly int MAX_COST = 1000;
        public static readonly Point[] aimPoints = { new Point(0, 0), new Point(0, 2), new Point(2, 0), new Point(0, -2) };
        public static readonly int TYPE_COUNT = aimPoints.Length;
        public Point aim, aimOffset;
        public Vector2 dirVector;
        public static readonly float MOVE_SPEED = Hero.MOVE_SPEED * 0.75f;
        public static LevelContent.LevelTile[,] original = new LevelContent.LevelTile[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public int[,] grid = new int[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public Point roundPosition;
        public int enemyUpdateFrequency = 10;
        public HashSet<Point> walls = new HashSet<Point>();
        public int[] pqCorrection = new int[2000];
        private readonly Dictionary<Vector2, Direction> VECTOR_TO_DIR = new Dictionary<Vector2, Direction>(){
            {Vector2.Zero,Direction.None},
            {new Vector2(0, -1),Direction.Up},
            {new Vector2(0, 1),Direction.Down},
            {new Vector2(-1, 0),Direction.Left},
            {new Vector2(1, 0),Direction.Right},
        };
        private Level lvl;
        private bool forceSandToSpawn;
        public static readonly int STARTING_HP = 5;
        public int hp;
        public int type;
        private int enemyUpdateCounter = 0;
        public bool dying = false;
        public Emitter emitter;
        public List<Bullet> bulletsAlreadyHit = new List<Bullet>();
        public static int idx = 0;
        public int id;

        public Enemy(int x, int y, int type, Level lv, bool forceSandToSpawn = false)
            : base(new Hitbox(32, 32))
        {
            lvl = lv;
            this.forceSandToSpawn = forceSandToSpawn;
            this.type = type;
            aimOffset = aimPoints[type];
            roundPosition = new Point(x, y);
            position = new Vector2(Level.TEX_SIZE * lvl.xPos + (x * Level.TILE_SIZE) + Level.TILE_SIZE / 2, Level.TEX_SIZE * lvl.yPos + (y * Level.TILE_SIZE) + Level.TILE_SIZE / 2);
            this.setTexture("enemy" + (type + 1));
            id = idx++;
            hp = (Game1.state == GameState.Arena) ? 3 : 5;
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
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y); //Manhattan distance
        }
        public Vector2 pathFinding(int pos)
        {
            enemyUpdateCounter++;
            enemyUpdateCounter %= enemyUpdateFrequency;
            if (enemyUpdateCounter != 0)
                return dirVector;
             
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
                return Vector2.Zero;
            }
            Vector2 path = lvl.aStar(roundPosition.X,roundPosition.Y,aim.X,aim.Y);
            if (path == Vector2.Zero)
                path = lvl.aStar(roundPosition.X, roundPosition.Y, Hero.instance.tileX, Hero.instance.tileY); // try without modifying aim
            return path;
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
                float moveSpeed = MOVE_SPEED * ((Game1.state == GameState.Arena) ? 0.75f : 1f);
                Vector2 movement = dirVector * moveSpeed * seconds;
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
            double chance = Game1.rand.NextDouble();
            if (chance < CHANCE_TO_SPAWN_SAND_ON_DEATH || forceSandToSpawn)
            {
                int i = roundPosition.X;
                int j = roundPosition.Y;
                lvl.collectables.Add(new Sand(Level.TEX_SIZE * lvl.xPos + i * Level.TILE_SIZE + 16, Level.TEX_SIZE * lvl.yPos + j * Level.TILE_SIZE + 16, lvl.xPos, lvl.yPos, i, j));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (dying)
                emitter.Draw(spriteBatch);
            else
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
