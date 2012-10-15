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
        public static readonly int TILE_SIZE = 32;
        public static readonly int TEX_SIZE = LevelContent.LEVEL_SIZE * TILE_SIZE;
        private Texture2D levelTextureDebug;
        public Texture2D levelTexture;
        public LevelContent.LevelTile[,] grid = new LevelContent.LevelTile[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public Collectable[,] collectables = new Collectable[LevelContent.LEVEL_SIZE, LevelContent.LEVEL_SIZE];
        public int xPos, yPos;
        public List<Enemy> enemies = new List<Enemy>();

        private static int idCounter = 0;
        public int id;

        public static Dictionary<LevelContent.LevelTile, Texture2D> TILE_TO_TEXTURE;
        public List<int[]> green = new List<int[]>();

        public static Texture2D texFloor = null, texWall = null;

        public static void Initialize(ContentManager Content)
        {
            texFloor = Content.Load<Texture2D>("Textures\\floor1");
            texWall = Content.Load<Texture2D>("Textures\\wall1");

            TILE_TO_TEXTURE = new Dictionary<LevelContent.LevelTile, Texture2D>(){
                {LevelContent.LevelTile.White, texFloor},
                {LevelContent.LevelTile.Blue, null},
                {LevelContent.LevelTile.Black, texWall},
                {LevelContent.LevelTile.Red, null},
                {LevelContent.LevelTile.Green, null},
                {LevelContent.LevelTile.Yellow, null},
                {LevelContent.LevelTile.Purple, null},
            };
        }

        public Level(LevelContent content, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < content.grid.Length; x++)
            {
                int i = x % LevelContent.LEVEL_SIZE;
                int j = x / LevelContent.LEVEL_SIZE;
                grid[i, j] = content.grid[x];
            }
            id = idCounter++;

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
                    if (TILE_TO_TEXTURE[grid[i, j]] == null)
                        continue;
                    TILE_TO_TEXTURE[grid[i, j]].GetData<Color>(tiledata);
                    levelTexture.SetData<Color>(0, new Rectangle(i * TILE_SIZE, j * TILE_SIZE, TILE_SIZE, TILE_SIZE), tiledata, 0, TILE_SIZE * TILE_SIZE);
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
            grid = (LevelContent.LevelTile[,])l.grid.Clone();
            collectables = (Collectable[,])l.collectables.Clone();
            id = l.id;
            xPos = x;
            yPos = y;
            ////put in collectables

            for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                {
                    if (grid[i, j] == LevelContent.LevelTile.Green)
                    {
                        int[] loc = new int[2];
                        loc[0] = i; loc[1] = j;
                        green.Add(loc);
                    }
                    else if (grid[i, j] == LevelContent.LevelTile.Purple)
                        enemies.Add(Game1.levelManager.addEnemy(i, j, 0, this));
                }
            ////
        }
        public void Update(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
                enemy.Update(gameTime);

        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 offset = new Vector2(xPos * (LevelContent.LEVEL_SIZE * TILE_SIZE), yPos * (LevelContent.LEVEL_SIZE * TILE_SIZE));
            spriteBatch.Draw(getTexture(), new Vector2(xPos * Level.TEX_SIZE, yPos * Level.TEX_SIZE), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
            //History.DrawLevel(spriteBatch, xPos, yPos);
            foreach (Collectable c in collectables)
                if (c != null)
                    c.Draw(spriteBatch);
            foreach (Enemy enemy in enemies)
                enemy.Draw(spriteBatch);
        }

        public Texture2D getTexture()
        {
            return levelTexture;
        }

        public Texture2D getDebugTexture()
        {
            return levelTextureDebug;
        }

        public void drillWall(int tileX, int tileY)
        {
            grid[tileX, tileY] = LevelPipeline.LevelContent.LevelTile.White;
            Color[] tiledata = new Color[TILE_SIZE * TILE_SIZE];
            texFloor.GetData<Color>(tiledata);
            levelTexture.SetData<Color>(0, new Rectangle(tileX * TILE_SIZE, tileY * TILE_SIZE, TILE_SIZE, TILE_SIZE), tiledata, 0, TILE_SIZE * TILE_SIZE);
        }
    }
}
