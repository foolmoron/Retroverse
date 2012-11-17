using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using LevelPipeline;

namespace Retroverse
{
    public class Level
    {
        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static int alphabetOffset1, alphabetOffset2;
        public static readonly double CHANCE_TO_SPAWN_SAND_ON_DRILL = 0.10;
        public static readonly int DRILL_WALL_SCORE = 150;
        public static readonly int TILE_SIZE = 32;
        public static readonly int TEX_SIZE = LevelContent.LEVEL_SIZE * TILE_SIZE;
        public static readonly Color DEFAULT_COLOR = Color.Red;
        private Texture2D levelTextureDebug;
        public Texture2D levelTexture;
        public string cellName;
        public Color color = Color.White;
        public LevelContent.LevelTile[,] grid = new LevelContent.LevelTile[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public Collectable[,] collectables = new Collectable[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public int xPos, yPos;
        public List<Enemy> enemies = new List<Enemy>();
        public List<Prisoner> prisoners = new List<Prisoner>();
        public Vector2[,,,] cost;
        public static readonly int WALL_COST = 100000;
        public static readonly int MAX_COST = 1000;
        public int[,] grid_cost = new int[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public bool alive = false;

        private static int idCounter = 0;
        public int id;

        public static Dictionary<LevelContent.LevelTile, Texture2D> TILE_TO_TEXTURE;
        public List<int[]> collectableLocations = new List<int[]>();
        public List<int[]> enemyLocations = new List<int[]>();
        public List<int[]> prisonerLocations = new List<int[]>();

        public static Texture2D texFloor = null, texWall = null;

        public static void Load(ContentManager Content)
        {
            texFloor = Content.Load<Texture2D>("Textures\\floor1");
            texWall = Content.Load<Texture2D>("Textures\\wall1");

            TILE_TO_TEXTURE = new Dictionary<LevelContent.LevelTile, Texture2D>(){
                {LevelContent.LevelTile.White, texFloor},
                {LevelContent.LevelTile.Blue, null},
                {LevelContent.LevelTile.Black, texWall},
                {LevelContent.LevelTile.Red, null},
                {LevelContent.LevelTile.Green, (Game1.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Yellow, null},
                {LevelContent.LevelTile.Purple, null},
            };
        }

        public static void Initialize()
        {
            alphabetOffset1 = Game1.rand.Next(ALPHABET.Length);
            alphabetOffset2 = Game1.rand.Next(ALPHABET.Length);
        }

        public Level(LevelContent content, String name, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < content.grid.Length; x++)
            {
                int i = x % LevelContent.LEVEL_SIZE;
                int j = x / LevelContent.LEVEL_SIZE;
                grid[i, j] = content.grid[x];
                if (grid[i, j] == LevelContent.LevelTile.Black)
                {
                    grid_cost[i, j] = WALL_COST;
                }
                else grid_cost[i, j] = 1;
            }

            cost = new Vector2[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
            UpdateCost();

            id = idCounter++;
            if (Game1.LEVEL_COLORS.ContainsKey(name))
                color = Game1.LEVEL_COLORS[name];
            else
                color = DEFAULT_COLOR;

            //create texture
            levelTextureDebug = new Texture2D(spriteBatch.GraphicsDevice, TEX_SIZE, TEX_SIZE);
            Color[] data = new Color[TEX_SIZE * TEX_SIZE];
            for (int d = 0; d < data.Length; d++)
            {
                int i = (d % (TEX_SIZE)) / TILE_SIZE;
                int j = (d / (TEX_SIZE)) / TILE_SIZE;
                data[d] = LevelContent.TILE_TO_COLOR[grid[i, j]];
            }
            levelTextureDebug.SetData(data);
            levelTexture = new Texture2D(spriteBatch.GraphicsDevice, TEX_SIZE, TEX_SIZE);
            Color[] tiledata = new Color[TILE_SIZE * TILE_SIZE];
            for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                {
                    if (TILE_TO_TEXTURE[grid[i, j]] != null)
                    {
                        TILE_TO_TEXTURE[grid[i, j]].GetData<Color>(tiledata);
                        levelTexture.SetData<Color>(0, new Rectangle(i * TILE_SIZE, j * TILE_SIZE, TILE_SIZE, TILE_SIZE), tiledata, 0, TILE_SIZE * TILE_SIZE);
                    }

                    if (grid[i, j] == LevelContent.LevelTile.Green)
                        collectableLocations.Add(new int[2] { i, j });
                    if (grid[i, j] == LevelContent.LevelTile.Purple)
                        enemyLocations.Add(new int[2] { i, j });
                    if (grid[i, j] == LevelContent.LevelTile.Yellow)
                        prisonerLocations.Add(new int[2] { i, j });
                }
        }

        // Copy level with offsets
        public Level(Level l, int x, int y)
        {
            levelTextureDebug = l.levelTextureDebug;
            levelTexture = new Texture2D(Game1.graphicsDevice, TEX_SIZE, TEX_SIZE);
            Color[] data = new Color[TEX_SIZE * TEX_SIZE];
            l.levelTexture.GetData<Color>(data);
            levelTexture.SetData<Color>(data);
            color = l.color;
            grid = (LevelContent.LevelTile[,])l.grid.Clone();
            collectables = (Collectable[,])l.collectables.Clone();
            grid_cost = (int[,])l.grid_cost.Clone();
            cost = (Vector2[,,,])l.cost.Clone();
            id = l.id;
            xPos = x;
            yPos = y;
            alive = true;
            collectableLocations = l.collectableLocations;
            enemyLocations = l.enemyLocations;
            prisonerLocations = l.prisonerLocations;
            if (!(xPos == LevelManager.STARTING_LEVEL.X && yPos == LevelManager.STARTING_LEVEL.Y))
                foreach (int[] loc in collectableLocations)
                    collectables[loc[0], loc[1]] = new Collectable(Level.TEX_SIZE * xPos + loc[0] * Level.TILE_SIZE + Level.TILE_SIZE / 2, Level.TEX_SIZE * yPos + loc[1] * Level.TILE_SIZE + Level.TILE_SIZE / 2, xPos, yPos, loc[0], loc[1]);
            foreach (int[] loc in enemyLocations)
                enemies.Add(Game1.levelManager.addEnemy(loc[0], loc[1], (int)Game1.rand.Next(4), this));
            foreach (int[] loc in prisonerLocations)
                prisoners.Add(Game1.levelManager.addPrisoner(loc[0], loc[1], new Color(Game1.rand.Next(255), Game1.rand.Next(255), Game1.rand.Next(255), 255) , this));

            int alpha1, alpha2;
            alpha1 = yPos / 26 + 15;
            if (alpha1 >= 26)
                alpha1 -= 26;
            alpha2 = yPos % 26 + alphabetOffset2;
            if (alpha2 >= 26)
            {
                alpha2 -= 26;
                alpha1++;
                alpha1 %= 26;
            }

            cellName = "" + xPos + ALPHABET[alpha1] + ALPHABET[alpha2];
        }

        public void UpdateEscape(GameTime gameTime)
        {
            bool heroInCurrentLevel = Hero.instance.levelX == xPos && Hero.instance.levelY == yPos;
            foreach (Enemy enemy in enemies)
                enemy.Update(gameTime, heroInCurrentLevel);
            if (prisoners.Count > 1)
                prisoners.Sort(new Comparison<Prisoner>((p1, p2) => { if (p1.collectedTime == p2.collectedTime) return 0; else if (p1.collectedTime > p2.collectedTime) return 1; else return -1; }));
            foreach (Prisoner p in prisoners)
                p.Update(gameTime);

        }

        public void UpdateRetro(GameTime gameTime)
        {
            if (prisoners.Count > 1)
                prisoners.Sort(new Comparison<Prisoner>((p1, p2) => { if (p1.collectedTime == p2.collectedTime) return 0; else if (p1.collectedTime > p2.collectedTime) return 1; else return -1; }));
            foreach (Prisoner p in prisoners)
                p.Update(gameTime);

        }

        public Texture2D getTexture()
        {
            return levelTexture;
        }

        public Texture2D getDebugTexture()
        {
            return levelTextureDebug;
        }

        public bool drillWall(int tileX, int tileY)
        {
            try
            {
                if (grid[tileX, tileY] != LevelContent.LevelTile.Black)
                    return true;
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }
            
            grid[tileX, tileY] = LevelContent.LevelTile.White;
            grid_cost[tileX, tileY] = 1;
            Color[] tiledata = new Color[TILE_SIZE * TILE_SIZE];
            texFloor.GetData<Color>(tiledata);
            levelTexture.SetData<Color>(0, new Rectangle(tileX * TILE_SIZE, tileY * TILE_SIZE, TILE_SIZE, TILE_SIZE), tiledata, 0, TILE_SIZE * TILE_SIZE);
            foreach (Enemy enemy in enemies)
                enemy.grid[tileX, tileY] = 1;
            if (Game1.rand.NextDouble() < CHANCE_TO_SPAWN_SAND_ON_DRILL)
            {
                int i = tileX;
                int j = tileY;
                collectables[i, j] = new Sand(Level.TEX_SIZE * xPos + i * Level.TILE_SIZE + 16, Level.TEX_SIZE * yPos + j * Level.TILE_SIZE + 16, xPos, yPos, i, j);
            }
            UpdateCost();
            Game1.addScore(DRILL_WALL_SCORE);
            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 offset = new Vector2(xPos * (LevelContent.LEVEL_SIZE * TILE_SIZE), yPos * (LevelContent.LEVEL_SIZE * TILE_SIZE));
            spriteBatch.Draw(getTexture(), new Vector2(xPos * Level.TEX_SIZE, yPos * Level.TEX_SIZE), null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
            //History.DrawLevel(spriteBatch, xPos, yPos);
            foreach (Collectable c in collectables)
                if (c != null)
                    c.Draw(spriteBatch);
            foreach (Enemy enemy in enemies)
                enemy.Draw(spriteBatch);
            foreach (Prisoner p in prisoners)
                p.Draw(spriteBatch);
        }
        public void UpdateCost()
        {
            /*
            Vector2[,] temp=new Vector2[LevelContent.LEVEL_SIZE,LevelContent.LEVEL_SIZE];
            Console.WriteLine("update has started");
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                {
                    temp = aStar(a, b);
                    for (int c = 0; c < LevelContent.LEVEL_SIZE; c++)
                        for (int d = 0; d < LevelContent.LEVEL_SIZE; d++)
                            cost[a, b, c, d] = temp[c, d];
                }*/
            /*
            Dictionary<Point, Dictionary<Point, double>> dist = new Dictionary<Point, Dictionary<Point, double>>();
            Dictionary<Point, Dictionary<Point,Point>> path = new Dictionary<Point, Dictionary<Point,Point>>();
            //Point[,] temp = new Point[LevelContent.LEVEL_SIZE * LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE * LevelContent.LEVEL_SIZE];
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
            {
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++){
                    dist.Add(new Point(a, b), new Dictionary<Point, double>());
                    for(int c=0;c<LevelContent.LEVEL_SIZE;c++)
                        for(int d=0;d<LevelContent.LEVEL_SIZE;d++)
                            dist[new Point(a,b)].Add(new Point(c,d),1000);
                }
            }
            */
            cost = new Vector2[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
            /*
            for (int a = 0; a < LevelContent.LEVEL_SIZE * LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE * LevelContent.LEVEL_SIZE; b++)
                {
                    for (int c = 0; c < LevelContent.LEVEL_SIZE * LevelContent.LEVEL_SIZE; c++)
                            cost[a, b, c, d] = aStar(a, b, c, d);
                }*/
            /*
            foreach (Point a in dist.Keys)
                foreach (Point b in dist.Keys)
                    foreach (Point c in dist.Keys)
                        if (dist[b][a] + dist[a][c] < dist[b][c])
                        {
                            dist[b][c] = dist[b][a] + dist[a][c];
                            path[b][c] = a;
                        }
             */
        }
        public double dist(Point first, Point second)
        {
            //return Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
            //return 10;
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y); //Manhattan distance
        }
        public Vector2 aStar(int x1, int y1,int x2,int y2)
        {
            //Console.WriteLine("astar has started");
            //LevelContent.LevelTile[,] grid=LevelManager.levels[LevelManager.STARTING_LEVEL.X,LevelManager.STARTING_LEVEL.Y].grid;
            
            //Vector2[,] temp=new Vector2[LevelContent.LEVEL_SIZE,LevelContent.LEVEL_SIZE];
            int[] pqCorrection = new int[2000];
            //aim = new Vector2(9,22);//
            Point roundPosition = new Point(x1, y1);
            Point aim = new Point(x2,y2);
            if(cost[x1, y1, x2, y2].Equals(new Vector2(-500,-500)))
                return Vector2.Zero;
            if (cost[x1, y1, x2, y2] != Vector2.Zero)
            {
                return cost[x1, y1, x2, y2];
            }
            /*
            if (aim.X >= 31 || aim.Y >= 31 || aim.X < 0 || aim.Y < 0)
                return Vector2.Zero;
            if (aim.Equals(roundPosition))
            {
                return Vector2.Zero;
            }
            */
            /*
            if (grid[(int)aim.X, (int)aim.Y] >= WALL_COST)
            {
                grid[(int)aim.X, (int)aim.Y] = 1;
                walls.Remove(aim);
            }
            */


            SortedList<double, Point> queue = new SortedList<double, Point>();
            HashSet<Point> visited = new HashSet<Point>();
            Dictionary<Point, Point> path = new Dictionary<Point, Point>();
            int[,] best_cost = new int[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                {
                    best_cost[a, b] = MAX_COST;
                }
            best_cost[(int)roundPosition.X, (int)roundPosition.Y] = 0;
            for (int a = 0; a < LevelContent.LEVEL_SIZE; a++)
                for (int b = 0; b < LevelContent.LEVEL_SIZE; b++)
                {
                    queue.Add(100 + a * LevelContent.LEVEL_SIZE + b, new Point(a, b));
                }

            queue.RemoveAt(queue.IndexOfValue(roundPosition));
            queue.Add(dist(roundPosition, aim), roundPosition);
            Point cur, neighbor;
            int score = MAX_COST;
            double cost_temp = 0;
            while (queue.Count > 0)
            {
                //Console.WriteLine(queue.Count);
                cur = queue.First().Value;

                //if(best_cost[cur.X,cur.Y]<temp[cur.X,cur.Y]
                //need to make this so basically if cur is known, the whole path is kown
                if (cur == aim || (!cost[cur.X, cur.Y, aim.X, aim.Y].Equals(Vector2.Zero)))
                {
                    if (best_cost[cur.X, cur.Y] >= MAX_COST|| cost[cur.X, cur.Y, aim.X, aim.Y].Equals(new Vector2(-500,-500)))
                    {
                        cost[x1, y1, x2, y2] = new Vector2(-500, -500);
                        return Vector2.Zero;
                    }
                    Point returnPoint = findPath(path, cur,roundPosition);
                    //cost[x1, y1, x2, y2] = Vector2.Normalize(new Vector2(returnPoint.X - roundPosition.X, returnPoint.Y - roundPosition.Y)) * best_cost[aim.X, aim.Y];
                    while (!returnPoint.Equals(cur))
                    {
                        returnPoint = findPath(path, cur, roundPosition);
                        //if(!cost[roundPosition.X, roundPosition.Y, aim.X, aim.Y].Equals(Vector2.Zero))
                            cost[roundPosition.X, roundPosition.Y, aim.X, aim.Y] =(new Vector2(returnPoint.X - roundPosition.X, returnPoint.Y - roundPosition.Y));// *best_cost[aim.X, aim.Y];
                        
                        roundPosition = returnPoint;
                    }
                    //return Vector2.Normalize(new Vector2(returnPoint.X - roundPosition.X, returnPoint.Y - roundPosition.Y))*best_cost[aim.X,aim.Y];
                    return cost[x1, y1, x2, y2];
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
                        score = best_cost[(int)cur.X, (int)cur.Y] + grid_cost[(int)cur.X + a, (int)cur.Y + b];
                        {
                            if ((!queue.ContainsValue(neighbor) || score < best_cost[(int)neighbor.X, (int)neighbor.Y]) && !path.ContainsKey(neighbor))
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
            cost[x1, y1, x2, y2] = new Vector2(-500, -500);
            return new Vector2(roundPosition.X, roundPosition.Y);
        }
        public Point findPath(Dictionary<Point, Point> dic, Point node,Point roundPosition)
        {
            //Console.WriteLine("loop?");
            if (dic.ContainsKey(node) && !roundPosition.Equals(dic[node]))
                return findPath(dic, dic[node],roundPosition);
            return node;
        }
    }
}
