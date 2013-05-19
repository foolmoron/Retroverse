using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class LevelFragment
    {
        public Texture2D levelTextureDebug;
        public Texture2D levelTexture;
        public Color color;

        public readonly string name;
        public readonly LevelContent.Type type;
        public LevelContent.LevelTile[,] grid;
        public List<int[]> collectableLocations = new List<int[]>();
        public List<int[]> enemyLocations = new List<int[]>();
        public List<int[]> prisonerLocations = new List<int[]>();
        public List<int[]> powerupLocations = new List<int[]>();
        public int[][][] specialTiles = new int[2][][] { new int[4][], new int[4][] };
        public int[][] heroTiles = new int[2][];

        private static int idCounter = 0;
        public int id;
        
        public LevelFragment(LevelContent content, GraphicsDevice graphicsDevice)
        {
            grid = new LevelContent.LevelTile[content.levelWidth, content.levelHeight];
            for (int x = 0; x < content.grid.Length; x++)
            {
                int i = x % content.levelWidth;
                int j = x / content.levelWidth;
                grid[i, j] = content.grid[x];
            }

            name = content.name;
            type = content.type;
            id = idCounter++;
            color = content.color;

            //create texture
            int texWidth = content.levelWidth * Level.TILE_SIZE;
            int texHeight = content.levelHeight * Level.TILE_SIZE;

            levelTextureDebug = new Texture2D(graphicsDevice, texWidth, texHeight);
            Color[] data = new Color[texWidth * texHeight];
            for (int d = 0; d < data.Length; d++)
            {
                int i = (d % (texWidth)) / Level.TILE_SIZE;
                int j = (d / (texWidth)) / Level.TILE_SIZE;
                data[d] = LevelContent.TILE_TO_COLOR[grid[i, j]];
            }
            levelTextureDebug.SetData(data);
            levelTexture = new Texture2D(graphicsDevice, texWidth, texHeight);
            Color[] tileData = new Color[Level.TILE_SIZE * Level.TILE_SIZE];
            for (int i = 0; i < content.levelWidth; i++)
                for (int j = 0; j < content.levelHeight; j++)
                {
                    if (Level.TILE_TO_TEXTURE[grid[i, j]] != null)
                    {
                        Level.TILE_TO_TEXTURE[grid[i, j]].GetData<Color>(tileData);
                        // tint texture 
                        for (int d = 0; d < tileData.Length; d++)
                            tileData[d] = tileData[d].Tint(color, Level.COLOR_TINT_FACTOR);
                        levelTexture.SetData<Color>(0, new Rectangle(i * Level.TILE_SIZE, j * Level.TILE_SIZE, Level.TILE_SIZE, Level.TILE_SIZE), tileData, 0, Level.TILE_SIZE * Level.TILE_SIZE);
                    }

                    if (grid[i, j] == LevelContent.LevelTile.Gem)
                        collectableLocations.Add(new int[2] { i, j });
                    else if (grid[i, j] == LevelContent.LevelTile.Enemy)
                        enemyLocations.Add(new int[2] { i, j });
                    else if (grid[i, j] == LevelContent.LevelTile.Prisoner)
                        prisonerLocations.Add(new int[2] { i, j });
                    else if (grid[i, j] == LevelContent.LevelTile.Powerup)
                        powerupLocations.Add(new int[2] { i, j });
                    //special tiles
                    else if(grid[i, j] == LevelContent.LevelTile.Special1)
                    {
                        if (specialTiles[0][0] == null) 
                            specialTiles[0][0] = new int[2] {i, j};
                        else
                            specialTiles[1][0] = new int[2] { i, j };
                    }
                    else if (grid[i, j] == LevelContent.LevelTile.Special2)
                    {
                        if (specialTiles[0][1] == null) 
                            specialTiles[0][1] = new int[2] { i, j };
                        else
                            specialTiles[1][1] = new int[2] { i, j };
                    }
                    else if (grid[i, j] == LevelContent.LevelTile.Special3)
                    {
                        if (specialTiles[0][2] == null) 
                            specialTiles[0][2] = new int[2] { i, j };
                        else
                            specialTiles[1][2] = new int[2] { i, j };
                    }
                    else if (grid[i, j] == LevelContent.LevelTile.Special4)
                    {
                        if (specialTiles[0][3] == null) 
                            specialTiles[0][3] = new int[2] { i, j };
                        else
                            specialTiles[1][3] = new int[2] { i, j };
                    }
                    else if (grid[i, j] == LevelContent.LevelTile.Hero1)
                        heroTiles[0] = new int[2] { i, j };
                    else if (grid[i, j] == LevelContent.LevelTile.Hero2)
                        heroTiles[1] = new int[2] { i, j };
                }
        }

        public List<int[]> getCollectableLocations(FragmentPosition fragmentPosition)
        {
            return getShiftedLocations(collectableLocations, fragmentPosition);
        }

        public List<int[]> getEnemyLocations(FragmentPosition fragmentPosition)
        {
            return getShiftedLocations(enemyLocations, fragmentPosition);
        }

        public List<int[]> getPrisonerLocations(FragmentPosition fragmentPosition)
        {
            return getShiftedLocations(prisonerLocations, fragmentPosition);
        }

        public List<int[]> getPowerupLocations(FragmentPosition fragmentPosition)
        {
            return getShiftedLocations(powerupLocations, fragmentPosition);
        }

        private List<int[]> getShiftedLocations(List<int[]> locations, FragmentPosition fragmentPosition)
        {
            List<int[]> ret = new List<int[]>();
            switch (fragmentPosition)
            {
                case FragmentPosition.LeftHalf:
                case FragmentPosition.TopHalf:
                case FragmentPosition.TopLeftCorner:
                    ret.AddRange(locations);
                    break;
                case FragmentPosition.RightHalf:
                case FragmentPosition.TopRightCorner:
                    foreach (int[] coord in locations)
                    {
                        int[] shifted = { coord[0] + LevelContent.LEVEL_SIZE_HALF - 1, coord[1] };
                        ret.Add(shifted);
                    }
                    break;
                case FragmentPosition.BottomHalf:
                case FragmentPosition.BottomLeftCorner:
                    foreach (int[] coord in locations)
                    {
                        int[] shifted = { coord[0], coord[1] + LevelContent.LEVEL_SIZE_HALF - 1 };
                        ret.Add(shifted);
                    }
                    break;
                case FragmentPosition.BottomRightCorner:
                    foreach (int[] coord in locations)
                    {
                        int[] shifted = { coord[0] + LevelContent.LEVEL_SIZE_HALF - 1, coord[1] + LevelContent.LEVEL_SIZE_HALF - 1 };
                        ret.Add(shifted);
                    }                    
                    break;
            }
            return ret;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(levelTexture, offset, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
        }
    }
}
