using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LevelPipeline;

namespace Retroverse
{
    public class LevelManager
    {
        public static readonly int MAX_LEVELS = 6;
        public static readonly Vector2 STARTING_LEVEL = new Vector2(0, 0);
        public static float SCREENWIDTH_MIN = 800;
        public static float SCREENWIDTH_MAX = 1333;
        public float screenWidth;
        public static float SCREENHEIGHT_MIN = 480;
        public static float SCREENHEIGHT_MAX = 800;
        public float screenHeight;

        public float zoom;
        public bool scrolling = false;

        public Vector2 position;
        public Vector2 targetPos;
        public static readonly float SCROLL_SPEED_DEFAULT = Hero.MOVE_SPEED * 1.20f;
        public float scrollSpeed = SCROLL_SPEED_DEFAULT;
        public float scrollMultiplier = 1f;
        public Vector2 acceleration;

        public Hero hero = new Hero();
        public bool heroLevelChanged = false;
        public Level[,] levels = new Level[MAX_LEVELS, MAX_LEVELS];

        public static float collectableElapsedTime = 0;
        public static readonly float COLLECTABLE_SPAWN_TIME = 1;

        // radar values
        public static int RADAR_BORDER_WIDTH = 2;
        public static int RADAR_CELL_WIDTHHEIGHT = 35;
        public static Color RADAR_BORDER_COLOR = Color.White;

        // entities to remove on next frame
        public List<Collectable> collectablesToRemove = new List<Collectable>();
        public List<Bullet> bulletsToRemove = new List<Bullet>();

        public LevelManager()
        {
            zoom = 1f; // 1 level
            screenWidth = SCREENWIDTH_MIN;
            screenHeight = MathHelper.Clamp(screenWidth / Game1.viewport.AspectRatio, SCREENHEIGHT_MIN, SCREENHEIGHT_MAX);
            position = new Vector2(hero.position.X - zoom * (Level.TEX_SIZE / 2), hero.position.Y - zoom * (Level.TEX_SIZE / 2));
        }

        public void addLevel(Level l, int xPos, int yPos)
        {
            if (xPos < 0 || xPos >= MAX_LEVELS || yPos < 0 || yPos >= MAX_LEVELS)
            {
                throw new ArgumentOutOfRangeException("xPos,yPos", "Position of level (" + xPos + ", " + yPos + ") is out of range.");
            }
            levels[xPos, yPos] = new Level(l, xPos, yPos);
           
        }
        public Enemy addEnemy(int x, int y, int type,Level l)
        {
            return new Enemy(x,y,type,l);
        }
        public Matrix getTranslation()
        {
            return Matrix.CreateTranslation(new Vector3(-position.X + 16, -(position.Y + Game1.hudSize) + 16, 0));
        }

        public Matrix getScale()
        {
            return Matrix.CreateScale(new Vector3(Game1.viewport.Width / (zoom * Level.TEX_SIZE), Game1.viewport.Height / (zoom * (Level.TEX_SIZE + Game1.hudSize)), 1));
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
            int x = (int) leadEdge.X;
            int y = (int) leadEdge.Y;
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

            //*************Jon****************
            if (hero.powerUp2 > 0) //should work then for all ghost, drill, etc.
                return true;
            //*****************************

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


            //*********Jon***************
            
            if (hero.powerUp2 == 1) 
            {
                return false;//Prevents hero from "snapping" to the walls when trying to go throught them
            }
          
            //************************

            switch (tile)
            {
                case LevelContent.LevelTile.Black:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //place collectables
            collectableElapsedTime += seconds;
            if (collectableElapsedTime >= COLLECTABLE_SPAWN_TIME)
            {
                collectableElapsedTime = 0;
                int levelX = Hero.instance.levelX;//levelX and levelY controll which level
                int levelY = Hero.instance.levelY;
                if (levels[levelX, levelY] != null)
                {
                    int rand = Game1.rand.Next(levels[levelX, levelY].green.Count);
                    if (levels[levelX, levelY].green.Count > 0)
                    {
                        int[] loc = levels[levelX, levelY].green[rand];
                        int i = loc[0];
                        int j = loc[1];
                        if (levels[levelX, levelY].collectables[i, j] == null)
                            levels[levelX, levelY].collectables[i, j] = new Collectable(Level.TEX_SIZE * levelX + i * Level.TILE_SIZE + Level.TILE_SIZE / 2, Level.TEX_SIZE * levelY + j * Level.TILE_SIZE + Level.TILE_SIZE / 2, levelX, levelY, i, j);
                    }
                }
            }

            //remove entities
            if(collectablesToRemove != null)
                foreach (Collectable c in collectablesToRemove)
                {
                    if (levels != null && levels[c.levelX, c.levelY] != null && levels[c.levelX, c.levelY].collectables != null)
                        levels[c.levelX, c.levelY].collectables[c.tileX, c.tileY] = null;
                }

            foreach (Bullet b in bulletsToRemove)
            {
                Hero.instance.ammo.Remove(b);
            }
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
                    //DRAW LEVEL
                    levels[x, y].Update(gameTime);
                }
            int curHeroLevelX = hero.levelX;
            int curHeroLevelY = hero.levelY;
            heroLevelChanged = !((prevHeroLevelX == curHeroLevelX) && (prevHeroLevelY == curHeroLevelY));
            if (curHeroLevelX < prevHeroLevelX) // hero transitioned left
            {
                int indexX = curHeroLevelX + 2;
                int indexY = curHeroLevelY;
                if (indexX < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            levels[indexX, indexY + i] = null;
                    }
                }
                indexX = curHeroLevelX - 1;
                if (indexX >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            levels[indexX, indexY + i] = new Level(Game1.levelTemplates["" + (Game1.rand.Next(Game1.levelTemplates.Keys.Count) + 1)], indexX, indexY + i);
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
                            levels[indexX, indexY + i] = null;
                    }
                }
                indexX = curHeroLevelX + 1;
                if (indexX < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                            levels[indexX, indexY + i] = new Level(Game1.levelTemplates["" + (Game1.rand.Next(Game1.levelTemplates.Keys.Count) + 1)], indexX, indexY + i);
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
                            levels[indexX + i, indexY] = null;
                    }
                }
                indexY = curHeroLevelY - 1;
                if (indexY >= 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            levels[indexX + i, indexY] = new Level(Game1.levelTemplates["" + (Game1.rand.Next(Game1.levelTemplates.Keys.Count) + 1)], indexX + i, indexY);
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
                            levels[indexX + i, indexY] = null;
                    }
                }
                indexY = curHeroLevelY + 1;
                if (indexY < MAX_LEVELS)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        if (indexX + i < MAX_LEVELS && indexX + i >= 0)
                            levels[indexX + i, indexY] = new Level(Game1.levelTemplates["" + (Game1.rand.Next(Game1.levelTemplates.Keys.Count) + 1)], indexX + i, indexY);
                    }
                }
            }

            foreach (Bullet b in Hero.instance.ammo)
            {
                b.Update(gameTime);
            }

            //Minh - update collectables
            for (int x = 0; x < MAX_LEVELS; x++)
                for (int y = 0; y < MAX_LEVELS; y++)
                {
                    if (levels[x, y] != null)
                        for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                            for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                            {
                                if (levels[x, y].collectables[i, j] != null)
                                {
                                    levels[x, y].collectables[i, j].Update(gameTime);
                                }
                            }
                }
            //
            float mult = 0f;
            if (Controller.isDown(Keys.Q))
                mult = 1f;
            else if (Controller.isDown(Keys.E))
                mult = -1f;
            zoom += zoom * mult * seconds;
            if (!scrolling)
                return;
            //scrolling
            //*****************Jon*******
            if (hero.powerUp1==0 || hero.powerUp1==3)
                scrollSpeed = Hero.MOVE_SPEED * scrollMultiplier *  1.20f;
            if (hero.powerUp1 == 1 || hero.powerUp1 == 2)  //Allows scrolling to catch up to hero when moving faster
                scrollSpeed = Hero.MOVE_SPEED * scrollMultiplier * 2.0f;
            //****************************
            targetPos = new Vector2(hero.position.X - zoom * (Level.TEX_SIZE / 2), hero.position.Y - zoom * (Level.TEX_SIZE / 2) - Game1.hudSize);
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

            foreach (Bullet b in Hero.instance.ammo)
            {
                b.Draw(spriteBatch);
            }

            hero.Draw(spriteBatch);
            if (Game1.DEBUG)
                hero.DrawDebug(spriteBatch);

            //foreach(Enemy enemy in enemies)
            //    enemy.Draw(spriteBatch);
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

        public void DrawRadar(SpriteBatch spriteBatch)
        {
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
                        switch (levels[x, y].id)
                        {
                            case 0:
                                c = Color.Purple;
                                break;
                            case 1:
                                c = Color.Green;
                                break;
                            case 2:
                                c = Color.Pink;
                                break;
                            case 3:
                                c = Color.Brown;
                                break;
                            case 4:
                                c = Color.Orange;
                                break;
                        }
                    }
                    c.A = (byte)(c.A * 0.8); // translucentize it
                    spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (RADAR_CELL_WIDTHHEIGHT * 3 + 20) + ((i + 1) * RADAR_CELL_WIDTHHEIGHT), 50 + ((j + 1) * RADAR_CELL_WIDTHHEIGHT), RADAR_CELL_WIDTHHEIGHT, RADAR_CELL_WIDTHHEIGHT), c);
                }
            float tilePercX = (float)hero.tileX / LevelContent.LEVEL_SIZE;
            float tilePercY = (float)hero.tileY / LevelContent.LEVEL_SIZE;
            spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)(Game1.screenSize.X - (RADAR_CELL_WIDTHHEIGHT * (2 - tilePercX) + 20)), (int)(50 + RADAR_CELL_WIDTHHEIGHT * (1 + tilePercY)), RADAR_BORDER_WIDTH * 2, RADAR_BORDER_WIDTH * 2), Color.Goldenrod);
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (RADAR_CELL_WIDTHHEIGHT * 3 + 20) + (i * RADAR_CELL_WIDTHHEIGHT), 50, RADAR_BORDER_WIDTH, (3 * RADAR_CELL_WIDTHHEIGHT) + RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)Game1.screenSize.X - (RADAR_CELL_WIDTHHEIGHT * 3 + 20), 50 + (i * RADAR_CELL_WIDTHHEIGHT), (3 * RADAR_CELL_WIDTHHEIGHT) + RADAR_BORDER_WIDTH, RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            float offsetX = 0;
            float offsetY = 0;
            for (int x = 0; x < MAX_LEVELS; x++)
                for (int y = 0; y < MAX_LEVELS; y++)
                {
                    if (levels[x, y] == null)
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