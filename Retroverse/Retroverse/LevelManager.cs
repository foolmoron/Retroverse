using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Particles;

namespace Retroverse
{
    public class LevelManager
    {
        public static readonly int MAX_LEVELS = 500;
        public static readonly Point STARTING_LEVEL = new Point(1, MAX_LEVELS / 2);
        public static readonly Point STARTING_TILE = new Point(15, 15);
        public static float SCREENWIDTH_MIN = 800;
        public static float SCREENWIDTH_MAX = 1333;
        public float screenWidth;
        public static float SCREENHEIGHT_MIN = 480;
        public static float SCREENHEIGHT_MAX = 800;
        public float screenHeight;

        private Entity centerEntity;
        public Vector2 center;

        public static readonly float ZOOM_ESCAPE = 1.1f;
        public static float zoomSpeed = 0.25f;
        public float targetZoom;
        public float zoom;
        public bool scrolling = false;

        public Vector2 position;
        public Vector2 targetPos;
        public static readonly float SCROLL_SPEED_DEFAULT = Hero.MOVE_SPEED * 2f;
        public float scrollSpeed = SCROLL_SPEED_DEFAULT;
        public float scrollMultiplier = 1f;
        public Vector2 acceleration;

        public Hero hero = new Hero();
        public bool heroLevelChanged = false;
        public Level[,] levels = new Level[MAX_LEVELS, MAX_LEVELS];

        // intro "cutscene" values
        public static bool introFinished;
        public static readonly float INTRO_INITIAL_ZOOM = 0.5f;
        public static readonly float INTRO_FINAL_ZOOM = 1f;
        public static readonly float INTRO_ZOOM_VELOCITY = 0.15f;
        public static readonly int[][] INTRO_WALLS_TO_CRUMBLE = new int[][] {
            new int[] { 16, 15 },
            new int[] { 16, 16 },
            new int[] { 16, 14 },
            new int[] { 15, 14 },
            new int[] { 15, 16 },
            new int[] { 14, 15 },
            new int[] { 14, 16 },
            new int[] { 14, 14 },
        };
        public static Emitter[] INTRO_WALLS_EMITTERS = new Emitter[8];

        // intro arena random collectable spawn values
        public static readonly int UNREACHABLE_COLLECTABLES = 8;
        public static float collectableElapsedTime = 0;
        public static readonly float COLLECTABLE_SPAWN_TIME = 1f;
        public static readonly int[] COLLECTABLE_LIMITS = new int[] { 15, 10, 10, 20, 30 };
        public static int collectableLimit = COLLECTABLE_LIMITS[0];
        public static int numCollectablesCurrentlyOnScreen = 0;
        public static int numCollectablesSpawnedThisPhase = 0;

        // radar values
        public static int RADAR_BORDER_WIDTH = 2;
        public static int RADAR_CELL_WIDTHHEIGHT = 20;
        public static Color RADAR_BORDER_COLOR = Color.White;
        public static Color RADAR_WALL_INDICATOR_COLOR = Color.HotPink;

        // entities to remove on next frame
        public List<Collectable> collectablesToRemove = new List<Collectable>();
        public List<Bullet> bulletsToRemove = new List<Bullet>();
        public List<Enemy> enemiesToRemove = new List<Enemy>();

        public LevelManager()
        {
            zoom = 1f; // 1 level
            targetZoom = 1f;
            screenWidth = SCREENWIDTH_MIN;
            screenHeight = MathHelper.Clamp(screenWidth / Game1.viewport.AspectRatio, SCREENHEIGHT_MIN, SCREENHEIGHT_MAX);
            position = new Vector2(hero.position.X - zoom * (Level.TEX_SIZE / 2), hero.position.Y - zoom * (Level.TEX_SIZE / 2));
            setCenterEntity(hero);
        }

        public void addLevel(Level l, int xPos, int yPos)
        {
            if (xPos < 0 || xPos >= MAX_LEVELS || yPos < 0 || yPos >= MAX_LEVELS)
            {
                throw new ArgumentOutOfRangeException("xPos,yPos", "Position of level (" + xPos + ", " + yPos + ") is out of range.");
            }
            levels[xPos, yPos] = new Level(l, xPos, yPos);

        }

        public void removeLevel(int xPos, int yPos)
        {
            if (levels[xPos, yPos] != null)
                levels[xPos, yPos].alive = false;
            levels[xPos, yPos] = null;
        }

        public void addEnemy(int x, int y, int type, Level l, bool forceSandToSpawn = false)
        {
            l.enemies.Add(new Enemy(x, y, type, l, forceSandToSpawn));
        }

        public Prisoner addPrisoner(int x, int y, Color c, Level l)
        {
            return new Prisoner(c, Names.getRandomName(), l.xPos * Level.TEX_SIZE + x * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + y * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.xPos, l.yPos, x, y);
        }

        public Matrix getTranslation()
        {
            return Matrix.CreateTranslation(new Vector3(-position.X + Level.TILE_SIZE / 2, -(position.Y) + Level.TILE_SIZE / 2, 0));
        }

        public Matrix getScale()
        {
            return Matrix.CreateScale(new Vector3(Game1.viewport.Width / (zoom * Level.TEX_SIZE), Game1.viewport.Height / (zoom * (Level.TEX_SIZE + Game1.levelOffsetFromHUD)), 1));
        }

        public Matrix getViewMatrix()
        {
            return getTranslation() * getScale();
        }

        public bool attemptScroll(Entity entity, Vector2 offset)
        {
            Vector2 topEdge = (entity.getTop() + offset);
            Vector2 bottomEdge = (entity.getBottom() + offset);
            Vector2 leftEdge = entity.getLeft() + offset;
            Vector2 rightEdge = entity.getRight() + offset;
            Vector2 leadEdge = Vector2.Zero;
            Direction dir = Direction.None;
            if (offset.X < 0 && offset.X != 0)
            {
                dir = Direction.Left;
                leadEdge = leftEdge;
            }
            else if (offset.X > 0 && offset.X != 0)
            {
                dir = Direction.Right;
                leadEdge = rightEdge;
            }
            else if (offset.Y < 0 && offset.Y != 0)
            {
                dir = Direction.Up;
                leadEdge = topEdge;
            }
            else if (offset.Y > 0 && offset.Y != 0)
            {
                dir = Direction.Down;
                leadEdge = bottomEdge;
            }
            int x = (int)leadEdge.X;
            int y = (int)leadEdge.Y;
            if (x <= 0 || y <= 0)
                return false;

            //if (dir == Direction.Left || dir == Direction.Right)
            //{
            //    if (x % Level.TILE_SIZE != 0)
            //        return true; // not aligned with wall, so cannot be colliding
            //}
            //else if (dir == Direction.Up || dir == Direction.Down)
            //{
            //    if (y % Level.TILE_SIZE != 0)
            //        return true; // not aligned with wall, so cannot be colliding
            //}

            int levelX = x / Level.TEX_SIZE; // get which level you are going to
            int levelY = y / Level.TEX_SIZE;
            if (levelX >= MAX_LEVELS || levelY >= MAX_LEVELS)
                return false;
            Level level = Game1.levelManager.levels[levelX, levelY];
            if (level == null)
                return false;

            int tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            int tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;
            LevelContent.LevelTile tile = level.grid[tileX, tileY];

            switch (tile)
            {
                case LevelContent.LevelTile.Black:
                    return false;
                default:
                    break;
            }
            return true;
        }

        public bool collidesWithWall(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            if (x <= 0 || y <= 0)
                return true;

            int levelX = x / Level.TEX_SIZE; // get which level you are in
            int levelY = y / Level.TEX_SIZE;
            if (levelX >= MAX_LEVELS || levelY >= MAX_LEVELS)
                return false;
            Level level = Game1.levelManager.levels[levelX, levelY];
            if (level == null)
                return true;

            int tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            int tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;
            LevelContent.LevelTile tile = level.grid[tileX, tileY];


            ////*********Jon***************

            //if (hero.powerUp2 == 1)
            //{
            //    return false;//Prevents hero from "snapping" to the walls when trying to go throught them
            //}

            ////************************

            switch (tile)
            {
                case LevelContent.LevelTile.Black:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public void initializeArena()
        {
            int i = 0;
            foreach (int[] tile in INTRO_WALLS_TO_CRUMBLE)
            {
                levels[STARTING_LEVEL.X, STARTING_LEVEL.Y].grid[tile[0], tile[1]] = LevelContent.LevelTile.Black;
                Emitter wallEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
                wallEmitter.position = new Vector2(STARTING_LEVEL.X * Level.TEX_SIZE + tile[0] * Level.TILE_SIZE + Level.TILE_SIZE / 2, STARTING_LEVEL.Y * Level.TEX_SIZE + tile[1] * Level.TILE_SIZE + Level.TILE_SIZE / 2);
                wallEmitter.active = false;
                INTRO_WALLS_EMITTERS[i++] = wallEmitter;
            }
            zoom = INTRO_INITIAL_ZOOM;
            targetZoom = 1f;
            position = new Vector2(STARTING_LEVEL.X * Level.TEX_SIZE + STARTING_TILE.X * Level.TILE_SIZE - zoom * (Level.TEX_SIZE / 2) + Level.TILE_SIZE / 2, STARTING_LEVEL.Y * Level.TEX_SIZE + STARTING_TILE.Y * Level.TILE_SIZE - zoom * (Level.TEX_SIZE / 2) - (Game1.levelOffsetFromHUD) + Level.TILE_SIZE / 2);
            numCollectablesCurrentlyOnScreen = 0;
            introFinished = false;
            scrollCamera(new Vector2((STARTING_LEVEL.X + 0.5f) * Level.TEX_SIZE, (STARTING_LEVEL.Y + 0.5f) * Level.TEX_SIZE), 100);
        }

        public void UpdateArena(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            if (!introFinished)
            {
                hero.Update(gameTime);
                scrolling = true;
                bool emitterActive = false;
                float emitterSize = 0f;
                if (zoom <= INTRO_FINAL_ZOOM)
                {
                    zoom += INTRO_ZOOM_VELOCITY * seconds;
                    if (zoom >= (INTRO_INITIAL_ZOOM + INTRO_FINAL_ZOOM) / 2)
                        emitterActive = true;
                    emitterSize = zoom / INTRO_FINAL_ZOOM / 2;
                }
                else
                {
                    zoom = INTRO_FINAL_ZOOM;
                    introFinished = true;
                    foreach (int[] tile in INTRO_WALLS_TO_CRUMBLE)
                    {
                        levels[STARTING_LEVEL.X, STARTING_LEVEL.Y].grid[tile[0], tile[1]] = LevelContent.LevelTile.Green;                        
                    }
                    emitterActive = false;
                }
                foreach (Emitter e in INTRO_WALLS_EMITTERS)
                {
                    e.active = emitterActive;
                    e.startSize = emitterSize;
                    e.Update(gameTime);
                }
                scrollCamera(seconds);
            }
            else
            {
                foreach (Emitter e in INTRO_WALLS_EMITTERS)
                    if (!e.isFinished())
                        e.Update(gameTime);
                scrolling = false;
                collectableElapsedTime += seconds;
                if (collectableElapsedTime >= COLLECTABLE_SPAWN_TIME && numCollectablesSpawnedThisPhase < collectableLimit)
                {
                    collectableElapsedTime = 0;
                    int levelX = Hero.instance.levelX;
                    int levelY = Hero.instance.levelY;
                    if (levels[levelX, levelY] != null)
                    {
                        int rand = Game1.rand.Next(levels[levelX, levelY].collectableLocations.Count);
                        if (levels[levelX, levelY].collectableLocations.Count > 0)
                        {
                            int[] loc = levels[levelX, levelY].collectableLocations[rand];
                            int i = loc[0];
                            int j = loc[1];
                            bool collision = false;
                            foreach (Collectable c in levels[levelX, levelY].collectables)
                            {
                                if (c.tileX == i && c.tileY == j)
                                    collision = true;
                            }
                            if (!collision)
                            {
                                levels[levelX, levelY].collectables.Add(new Collectable(Level.TEX_SIZE * levelX + i * Level.TILE_SIZE + 16, Level.TEX_SIZE * levelY + j * Level.TILE_SIZE + 16, levelX, levelY, i, j));
                                numCollectablesCurrentlyOnScreen++;
                                numCollectablesSpawnedThisPhase++;
                            }
                        }
                    }
                }
                UpdateEscape(gameTime);
           }
        }

        public void UpdateEscape(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            //remove entities
            if (collectablesToRemove != null)
                foreach (Collectable c in collectablesToRemove)
                {
                    if (levels != null)
                    {
                        Level l = levels[c.levelX, c.levelY];
                        if (l != null && l.collectables != null)
                            if (c is Prisoner)
                            {
                                l.removePrisoner((Prisoner) c);
                            }
                            else
                            {
                                l.collectables.Remove(c);
                                if (!(c is Sand))
                                    numCollectablesCurrentlyOnScreen--;
                            }
                    }
                }
            collectablesToRemove.Clear();
            if (bulletsToRemove != null)
                foreach (Bullet b in bulletsToRemove)
                {
                    Hero.instance.ammo.Remove(b);
                }
            bulletsToRemove.Clear();
            if (enemiesToRemove != null)
                foreach (Enemy e in enemiesToRemove)
                {
                    foreach (Level l in levels)
                    {
                        if (l != null)
                        {
                            l.enemies.Remove(e);
                        }
                    }
                }
            enemiesToRemove.Clear();

            int prevHeroLevelX = hero.levelX;
            int prevHeroLevelY = hero.levelY;
            hero.Update(gameTime);
            for (int xi = -1; xi <= 1; xi++)
                for (int yi = -1; yi <= 1; yi++)
                {
                    int x = Hero.instance.levelX + xi;
                    int y = Hero.instance.levelY + yi;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null)
                        continue;
                    levels[x, y].UpdateEscape(gameTime);
                }
            int curHeroLevelX = hero.levelX;
            int curHeroLevelY = hero.levelY;
            heroLevelChanged = !((prevHeroLevelX == curHeroLevelX) && (prevHeroLevelY == curHeroLevelY));
            createAndRemoveLevels(prevHeroLevelX, prevHeroLevelY, curHeroLevelX, curHeroLevelY);

            foreach (Bullet b in Hero.instance.ammo)
            {
                b.Update(gameTime);
            }

            //Minh - update collectables
            for (int xi = -1; xi <= 1; xi++)
                for (int yi = -1; yi <= 1; yi++)
                {
                    int x = Hero.instance.levelX + xi;
                    int y = Hero.instance.levelY + yi;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null)
                        continue;
                    if (levels[x, y] != null)
                        foreach (Collectable c in levels[x, y].collectables)
                            c.Update(gameTime);
                }
#if DEBUG
            float mult = 0f;
            if (Controller.isDown(Keys.E))
                mult = 1f;
            else if (Controller.isDown(Keys.R))
                mult = -1f;
            targetZoom += zoomSpeed * mult * seconds;
#endif
            if (targetZoom - zoom > seconds * zoomSpeed)
                zoom += zoomSpeed * seconds;
            else if (targetZoom - zoom < -seconds * zoomSpeed)
                zoom -= zoomSpeed * seconds;
            else
                zoom = targetZoom;
            scrollCamera(seconds);
            calculateCenter();
        }

        public void UpdateRetro(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            scrollCamera(seconds);
            // Remove non-revertible collectables still
            if (collectablesToRemove != null)
                foreach (Collectable c in collectablesToRemove)
                {
                    if (levels != null)
                    {
                        Level l = levels[c.levelX, c.levelY];
                        if (l != null && l.collectables != null)
                            if (c is Prisoner)
                            {
                                l.removePrisoner((Prisoner)c);
                            }
                            else
                            {
                                if (!(c is Sand))
                                    numCollectablesCurrentlyOnScreen--;
                            }
                    }
                }
            collectablesToRemove.Clear();
            // Update non-revertible collectables still
            for (int xi = -1; xi <= 1; xi++)
                for (int yi = -1; yi <= 1; yi++)
                {
                    int x = Hero.instance.levelX + xi;
                    int y = Hero.instance.levelY + yi;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null)
                        continue;
                    if (levels[x, y] != null)
                    {
                        levels[x, y].UpdateRetro(gameTime);
                        foreach (Collectable c in levels[x, y].collectables)
                            c.Update(gameTime);
                    }
                }

            int prevHeroLevelX = hero.levelX;
            int prevHeroLevelY = hero.levelY;
            hero.setCurrentLevelAndTile();
            int curHeroLevelX = hero.levelX;
            int curHeroLevelY = hero.levelY;
            heroLevelChanged = !((prevHeroLevelX == curHeroLevelX) && (prevHeroLevelY == curHeroLevelY));
            createAndRemoveLevels(prevHeroLevelX, prevHeroLevelY, curHeroLevelX, curHeroLevelY);
            calculateCenter();
        }

        private void calculateCenter()
        {
            center.X = (centerEntity.position.X - position.X + Level.TILE_SIZE / 2) / (Level.TEX_SIZE * zoom);
            center.Y = (centerEntity.position.Y - position.Y + Level.TILE_SIZE / 2) / ((Level.TEX_SIZE + Game1.levelOffsetFromHUD) * zoom);
        }

        public void setCenterEntity(Entity e)
        {
            centerEntity = e;
            calculateCenter();
        }

        private void scrollCamera(float seconds)
        {
            if (scrolling)
                scrollCamera(hero.position, seconds);
            else
                scrollCamera(new Vector2((STARTING_LEVEL.X + 0.5f) * Level.TEX_SIZE, (STARTING_LEVEL.Y + 0.5f) * Level.TEX_SIZE), seconds);
        }

        public void scrollCamera(Vector2 destination, float seconds)
        {
            targetPos = new Vector2(destination.X - zoom * (Level.TEX_SIZE / 2) + Level.TILE_SIZE / 2, destination.Y - zoom * (Level.TEX_SIZE / 2) - (Game1.levelOffsetFromHUD) + Level.TILE_SIZE / 2);
            if (targetPos.X - position.X > seconds * scrollSpeed)
                position.X += scrollSpeed * seconds;
            else if (targetPos.X - position.X < -seconds * scrollSpeed)
                position.X -= scrollSpeed * seconds;
            else
                position.X = targetPos.X;
            if (targetPos.Y - position.Y > seconds * scrollSpeed)
                position.Y += scrollSpeed * seconds;
            else if (targetPos.Y - position.Y < -seconds * scrollSpeed)
                position.Y -= scrollSpeed * seconds;
            else
                position.Y = targetPos.Y;
            if (position == targetPos)
                scrollMultiplier = 1f;
        }

        public void createAndRemoveLevels(int prevHeroLevelX, int prevHeroLevelY, int curHeroLevelX, int curHeroLevelY)
        {
            if (curHeroLevelX < prevHeroLevelX) // hero transitioned left
            {
                int indexX = curHeroLevelX + 2;
                int indexY = curHeroLevelY;
                if (indexX < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            removeLevel(indexX, indexY + i);
                    }
                }
                indexX = curHeroLevelX - 1;
                if (indexX >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            levels[indexX, indexY + i] = new Level(Game1.levelTemplates.ElementAt(Game1.rand.Next(Game1.levelTemplates.Keys.Count)).Value, indexX, indexY + i);
                    }
                }
            }
            else if (curHeroLevelX > prevHeroLevelX) // hero transitioned right
            {
                int indexX = curHeroLevelX - 2;
                int indexY = curHeroLevelY;
                if (indexX >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            removeLevel(indexX, indexY + i);
                    }
                }
                indexX = curHeroLevelX + 1;
                if (indexX < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            levels[indexX, indexY + i] = new Level(Game1.levelTemplates.ElementAt(Game1.rand.Next(Game1.levelTemplates.Keys.Count)).Value, indexX, indexY + i);
                    }
                }
            }
            else if (curHeroLevelY < prevHeroLevelY) // hero transitioned up
            {
                int indexX = curHeroLevelX;
                int indexY = curHeroLevelY + 2;
                if (indexY < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            removeLevel(indexX + i, indexY);
                    }
                }
                indexY = curHeroLevelY - 1;
                if (indexY >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            levels[indexX + i, indexY] = new Level(Game1.levelTemplates.ElementAt(Game1.rand.Next(Game1.levelTemplates.Keys.Count)).Value, indexX + i, indexY);
                    }
                }
            }
            else if (curHeroLevelY > prevHeroLevelY) // hero transitioned down
            {
                int indexX = curHeroLevelX;
                int indexY = curHeroLevelY - 2;
                if (indexY >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            removeLevel(indexX + i, indexY);
                    }
                }
                indexY = curHeroLevelY + 1;
                if (indexY < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            levels[indexX + i, indexY] = new Level(Game1.levelTemplates.ElementAt(Game1.rand.Next(Game1.levelTemplates.Keys.Count)).Value, indexX + i, indexY);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int xi = -1; xi <= 1; xi++)
                for (int yi = -1; yi <= 1; yi++)
                {
                    int x = Hero.instance.levelX + xi;
                    int y = Hero.instance.levelY + yi;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null)
                        continue;
                    //DRAW LEVEL
                    levels[x, y].Draw(spriteBatch);
                }
            //History.DrawEnemies(spriteBatch);

            foreach (Bullet b in Hero.instance.ammo)
            {
                b.Draw(spriteBatch);
            }

            hero.Draw(spriteBatch);
            if (Game1.DEBUG)
                hero.DrawDebug(spriteBatch);

            if (Game1.state == GameState.Arena || Game1.state == GameState.StartScreen || Game1.state == GameState.PauseScreen)
            {
                if (!introFinished)
                {
                    foreach (int[] wall in INTRO_WALLS_TO_CRUMBLE)
                        spriteBatch.Draw(Level.TILE_TO_TEXTURE[LevelContent.LevelTile.Black], new Vector2(STARTING_LEVEL.X * Level.TEX_SIZE + wall[0] * Level.TILE_SIZE, STARTING_LEVEL.Y * Level.TEX_SIZE + wall[1] * Level.TILE_SIZE), null, Game1.LEVEL_COLORS["intro"], 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                foreach (Emitter e in INTRO_WALLS_EMITTERS)
                    if (!e.isFinished())
                        e.Draw(spriteBatch);
            }
        }

        private static bool pointsAdjacent(int x1, int y1, int x2, int y2)
        {
            if (Math.Abs(x1 - x2) > 1 || Math.Abs(y1 - y2) > 1)
                return false;
            if (x2 == x1 || x2 - 1 == x1 || x2 + 1 == x1)
            {
                if (y2 == y1 || y2 + 1 == y1 || y2 - 1 == y1)
                    return true;
            }
            return false;
        }

        public void DrawRadar(SpriteBatch spriteBatch, float hudScale)
        {
            int radarBaseHeight = (int)(1.2f * Game1.hudSize);
            int radarCellWidthHeight = (int)(RADAR_CELL_WIDTHHEIGHT * hudScale);
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    Color c = Color.Transparent;
                    int x = hero.levelX + i;
                    int y = hero.levelY + j;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS)
                        c = Color.Black;
                    else if (levels[x, y] == null)
                        c = Color.Black;
                    else
                    {
                        c = levels[x, y].color;
                    }
                    c.A = (byte)(c.A * 0.8); // translucentize it
                    spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (radarCellWidthHeight * 3 + 20) + ((i + 1) * radarCellWidthHeight), radarBaseHeight + ((j + 1) * radarCellWidthHeight), radarCellWidthHeight, radarCellWidthHeight), c);
                }
            float tilePercX = (float)hero.tileX / LevelContent.LEVEL_SIZE;
            float tilePercY = (float)hero.tileY / LevelContent.LEVEL_SIZE;
            float heroIndicatorX = (Game1.screenSize.X - (radarCellWidthHeight * (2 - tilePercX) + 20));
            float heroIndicatorY = (radarBaseHeight + radarCellWidthHeight * (1 + tilePercY));
            spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)heroIndicatorX, (int)heroIndicatorY, RADAR_BORDER_WIDTH * 2, RADAR_BORDER_WIDTH * 2), Color.Gold);
            float wallOffset = RiotGuardWall.wallPosition - Hero.instance.position.X;
            float wallOffsetPixels = wallOffset * radarCellWidthHeight / (float)Level.TEX_SIZE;
            int wallCurrentLevel = (int) RiotGuardWall.wallPosition / Level.TEX_SIZE;
            int wallLevelsBehind = hero.levelX - wallCurrentLevel;
            if (wallLevelsBehind <= 1)
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)(heroIndicatorX + wallOffsetPixels), radarBaseHeight, RADAR_BORDER_WIDTH * 3, (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH), RADAR_WALL_INDICATOR_COLOR);
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (radarCellWidthHeight * 3 + 20) + (i * radarCellWidthHeight), radarBaseHeight, RADAR_BORDER_WIDTH, (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (radarCellWidthHeight * 3 + 20), radarBaseHeight + (i * radarCellWidthHeight), (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH, RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            float offsetX = 0;
            float offsetY = 0;
            for (int xi = -1; xi <= 1; xi++)
                for (int yi = -1; yi <= 1; yi++)
                {
                    int x = Hero.instance.levelX + xi;
                    int y = Hero.instance.levelY + yi;
                    if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null)
                        continue;
                    offsetX = levels[x, y].xPos * Level.TEX_SIZE;
                    offsetY = levels[x, y].yPos * Level.TEX_SIZE;
                    // draw level background
                    spriteBatch.Draw(levels[x, y].getDebugTexture(), new Vector2(offsetX, offsetY), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
        }

        public void DrawDebugHUD(SpriteBatch spriteBatch)
        {
        }
    }
}