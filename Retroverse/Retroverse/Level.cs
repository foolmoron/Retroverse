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

namespace Retroverse
{
    public class Level : IReversible
    {
        #region Level type probabilities
        public static readonly double[] LEVEL_TYPE_PROBABILITIES =
        {
            1/8.0,
            // _ _
            //|   |
            //|_ _|
            1/8.0,
            // _ _
            //| | |
            //|_|_|
            1/8.0,
            // _ _
            //| |_|
            //|_|_|
            1/8.0,
            // _ _
            //|_| |
            //|_|_|
            1/8.0,
            // _ _
            //|___|
            //|_ _|
            1/8.0,
            // _ _
            //|_|_|
            //|_ _|
            1/8.0,
            // _ _
            //|___|
            //|_|_|
            1/8.0,
            // _ _
            //|_|_|
            //|_|_|
        };
        #endregion
        public static readonly int LEVEL_CLEAR_BONUS_SCORE = 10000;
        public static readonly double CHANCE_TO_SPAWN_SAND_ON_DRILL = 0.025;
        public static readonly int DRILL_WALL_SCORE = 150;
        public const int TILE_SIZE = 32;
        public const int GRID_SIZE = LevelContent.LEVEL_SIZE - 1;
        public const int TEX_SIZE = GRID_SIZE * TILE_SIZE;
        public const float COLOR_TINT_FACTOR = 1f;
        public const float WALL_TINT_FACTOR = 0.9f;
        public LevelManager levelManager;
        public LevelFragment[,] fragmentGrid = new LevelFragment[2, 2];
        public Texture2D levelOverlayTexture;
        public List<Point> drilledWalls = new List<Point>();
        public static readonly Color[] texData = new Color[TEX_SIZE * TEX_SIZE];
        public string cellName;
        public LevelContent.LevelTile[,] grid = new LevelContent.LevelTile[GRID_SIZE, GRID_SIZE];
        public List<Collectable> collectables = new List<Collectable>();
        public int xPos, yPos;
        public Enemy[,] enemyGrid = new Enemy[GRID_SIZE, GRID_SIZE];
        public List<Enemy> enemies = new List<Enemy>();
        public List<Prisoner> prisoners = new List<Prisoner>();
        public List<PowerupIcon> powerups = new List<PowerupIcon>();
        public bool alive = false;

        public Pathfinding pathfinding;

        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static int alphabetOffset1, alphabetOffset2;

        public static Dictionary<LevelContent.LevelTile, Texture2D> TILE_TO_TEXTURE;
        public List<int[]> collectableLocations = new List<int[]>();
        public List<int[]> enemyLocations = new List<int[]>();
        public List<int[]> prisonerLocations = new List<int[]>();
        public List<int[]> powerupLocations = new List<int[]>();

        public static Texture2D texFloor, texWall, texWallHoriz, texWallVert;

        public Color?[,] borderCornerColors = new Color?[2,2];
        public Color?[,] borderVertColors = new Color?[2,2];
        public Color?[,] borderHorizColors = new Color?[2,2];
        public enum BorderWall { Single, Vert, Horiz }
        public const int BORDER_WALL_LENGTH = LevelContent.LEVEL_SIZE/2 - 1;

        public static void Load(ContentManager Content)
        {
            texFloor = Content.Load<Texture2D>("Textures\\floor1");
            texWall = Content.Load<Texture2D>("Textures\\wall1");
            texWallHoriz = Content.Load<Texture2D>("Textures\\wallhoriz");
            texWallVert = Content.Load<Texture2D>("Textures\\wallvert");

            TILE_TO_TEXTURE = new Dictionary<LevelContent.LevelTile, Texture2D>(){
                {LevelContent.LevelTile.Floor, texFloor},
                {LevelContent.LevelTile.Blue, null},
                {LevelContent.LevelTile.Wall, texWall},
                {LevelContent.LevelTile.Red, null},
                {LevelContent.LevelTile.Gem, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Prisoner, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Enemy, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Powerup, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Special1, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Special2, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Special3, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Special4, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Hero1, (RetroGame.DEBUG) ? null : texFloor},
                {LevelContent.LevelTile.Hero2, (RetroGame.DEBUG) ? null : texFloor},
            };
        }

        public static void Initialize(int cellOffset1 = -1, int cellOffset2 = -1)
        {
            alphabetOffset1 = (cellOffset1 < 0) ? RetroGame.rand.Next(ALPHABET.Length) : cellOffset1;
            alphabetOffset2 = (cellOffset2 < 0) ? RetroGame.rand.Next(ALPHABET.Length) : cellOffset2;
            double probTotal = LEVEL_TYPE_PROBABILITIES.Sum();
            if (probTotal < 1.0)
            {
                LEVEL_TYPE_PROBABILITIES[0] += 1.0 - probTotal;
            }
        }

        public static string GetCellName(int xPos, int yPos, int cellOffset1 = -1, int cellOffset2 = -1)
        {
            if (RetroGame.TopLevelManagerScreen is StoreScreen)
                return "???";
            if (cellOffset1 < 0)
                cellOffset1 = alphabetOffset1;
            if (cellOffset2 < 0)
                cellOffset2 = alphabetOffset2;
            int alpha1 = yPos / 26 + cellOffset1;
            if (alpha1 >= 26)
                alpha1 -= 26;
            int alpha2 = yPos % 26 + cellOffset2;
            if (alpha2 >= 26)
            {
                alpha2 -= 26;
                alpha1++;
                alpha1 %= 26;
            }
            return "" + xPos + ALPHABET[alpha1] + ALPHABET[alpha2];
        }

        public override string ToString() { return "Level:[" + xPos + ", " + yPos + "]"; }

        #region Level Constructors
        public Level(LevelManager levelManager, int x, int y, int levelLayout = -1)
        {
            this.levelManager = levelManager;
            if (levelLayout < 0)
            {
                levelLayout = 0;
                double rand = RetroGame.rand.NextDouble();
                double probSum = 0;
                while (rand > (probSum + LEVEL_TYPE_PROBABILITIES[levelLayout]))
                {
                    probSum += LEVEL_TYPE_PROBABILITIES[levelLayout];
                    levelLayout++;
                }
            }
            createLevel(x, y, levelLayout);
        }

        public Level(LevelManager levelManager, LevelFragment fullLevelFragment, int x, int y)
        {
            this.levelManager = levelManager;
            levelFull(fullLevelFragment, x, y);
        }

        private void createLevel(int x, int y, int levelLayout)
        {
            switch (levelLayout)
            {
                case 0:
                    levelFull(RetroGame.getRandomLevelFragment(LevelContent.Type.Full), x, y);
                    break;
                case 1:
                    levelTwoVert(RetroGame.getRandomLevelFragment(LevelContent.Type.HalfVertical), RetroGame.getRandomLevelFragment(LevelContent.Type.HalfVertical), x, y);
                    break;
                case 2:
                    levelLeftVertTwoCorner(RetroGame.getRandomLevelFragment(LevelContent.Type.HalfVertical), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), x, y);
                    break;
                case 3:
                    levelRightVertTwoCorner(RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.HalfVertical), x, y);
                    break;
                case 4:
                    levelTwoHoriz(RetroGame.getRandomLevelFragment(LevelContent.Type.HalfHorizontal), RetroGame.getRandomLevelFragment(LevelContent.Type.HalfHorizontal), x, y);
                    break;
                case 5:
                    levelTopHorizTwoCorner(RetroGame.getRandomLevelFragment(LevelContent.Type.HalfHorizontal), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), x, y);
                    break;
                case 6:
                    levelBottomHorizTwoCorner(RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.HalfHorizontal), x, y);
                    break;
                case 7:
                    levelFourCorner(RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), RetroGame.getRandomLevelFragment(LevelContent.Type.Corner), x, y);
                    break;
                default:
                    throw new ArgumentException("levelLayout needs to be between 0 and 7", "levelLayout");
            }
        }

        // Helper function to set parts of the level
        private void setFragmentIntoLevel(LevelFragment fragment, FragmentPosition position)
        {
            int texWidth = fragment.levelTexture.Width;
            int texHeight = fragment.levelTexture.Height;
            int gridWidth = fragment.grid.GetLength(0);
            int gridHeight = fragment.grid.GetLength(1);
            int texStartWidth = 0;
            int texStartHeight = 0;
            int gridStartWidth = 0;
            int gridStartHeight = 0;
            switch (position)
            {
                case FragmentPosition.LeftHalf:
                case FragmentPosition.TopHalf:
                    texStartWidth = 0;
                    texStartHeight = 0;
                    gridStartWidth = 0;
                    gridStartHeight = 0;
                    break;
                case FragmentPosition.TopLeftCorner:
                    texStartWidth = 0;
                    texStartHeight = 0;
                    gridStartWidth = 0;
                    gridStartHeight = 0;
                    break;
                case FragmentPosition.RightHalf:
                case FragmentPosition.TopRightCorner:
                    texStartWidth = texWidth + TILE_SIZE;
                    texStartHeight = 0;
                    gridStartWidth = gridWidth - 1;
                    gridStartHeight = 0;
                    break;
                case FragmentPosition.BottomHalf:
                case FragmentPosition.BottomLeftCorner:
                    texStartWidth = 0;
                    texStartHeight = texHeight + TILE_SIZE;
                    gridStartWidth = 0;
                    gridStartHeight = gridHeight - 1;
                    break;
                case FragmentPosition.BottomRightCorner:
                    texStartWidth = texWidth + TILE_SIZE;
                    texStartHeight = texHeight + TILE_SIZE;
                    gridStartWidth = gridWidth - 1;
                    gridStartHeight = gridHeight - 1;
                    break;
            }
            //cut off last row/column if necessary
            switch (position)
            {
                case FragmentPosition.LeftHalf:
                case FragmentPosition.BottomLeftCorner:
                    gridHeight--;
                    break;
                case FragmentPosition.TopHalf:
                case FragmentPosition.TopRightCorner:
                    gridWidth--;
                    break;
                case FragmentPosition.RightHalf:
                case FragmentPosition.BottomHalf:
                case FragmentPosition.BottomRightCorner:
                    gridWidth--;
                    gridHeight--;
                    break;
            }

            //color grid
            switch (position)
            {
                case FragmentPosition.LeftHalf:
                    fragmentGrid[0, 0] = fragment;
                    fragmentGrid[0, 1] = fragment;
                    break;
                case FragmentPosition.RightHalf:
                    fragmentGrid[1, 0] = fragment;
                    fragmentGrid[1, 1] = fragment;
                    break;
                case FragmentPosition.TopHalf:
                    fragmentGrid[0, 0] = fragment;
                    fragmentGrid[1, 0] = fragment;
                    break;
                case FragmentPosition.BottomHalf:
                    fragmentGrid[0, 1] = fragment;
                    fragmentGrid[1, 1] = fragment;
                    break;
                case FragmentPosition.TopLeftCorner:
                    fragmentGrid[0, 0] = fragment;
                    break;
                case FragmentPosition.TopRightCorner:
                    fragmentGrid[1, 0] = fragment;
                    break;
                case FragmentPosition.BottomLeftCorner:
                    fragmentGrid[0, 1] = fragment;
                    break;
                case FragmentPosition.BottomRightCorner:
                    fragmentGrid[1, 1] = fragment;
                    break;
            }
            //level grid
            grid.setDataRectangle<LevelContent.LevelTile>(new Rectangle(gridStartWidth, gridStartHeight, gridWidth, gridHeight), fragment.grid);
            //locations
            collectableLocations.AddRange(fragment.getCollectableLocations(position));
            enemyLocations.AddRange(fragment.getEnemyLocations(position));
            prisonerLocations.AddRange(fragment.getPrisonerLocations(position));
            powerupLocations.AddRange(fragment.getPowerupLocations(position));
        }
        
        // Helper functions to set tiles in the level to walls
        private void setWallIntoLevel(BorderWall wallType, int wallIndexX, int wallIndexY)
        {
            int tileX = 0, tileY = 0;
            switch (wallType)
            {
                case BorderWall.Single:
                    tileX = wallIndexX * (BORDER_WALL_LENGTH + 1);
                    tileY = wallIndexY * (BORDER_WALL_LENGTH + 1);
                    borderCornerColors[wallIndexX, wallIndexY] = Color.Lerp(Color.White, getTileColor(tileX, tileY), COLOR_TINT_FACTOR);
                    grid[tileX, tileY] = LevelContent.LevelTile.Wall;
                    pathfinding.costGrid[tileX, tileY] = Pathfinding.COST_WALL;
                    break;
                case BorderWall.Horiz:
                    tileX = wallIndexX * (BORDER_WALL_LENGTH + 1) + 1;
                    tileY = wallIndexY * (BORDER_WALL_LENGTH + 1);
                    borderHorizColors[wallIndexX, wallIndexY] = Color.Lerp(Color.White, getTileColor(tileX, tileY), COLOR_TINT_FACTOR);
                    for (int i = 0; i < BORDER_WALL_LENGTH; i++)
                    {
                        grid[tileX + i, tileY] = LevelContent.LevelTile.Wall;
                        pathfinding.costGrid[tileX + i, tileY] = Pathfinding.COST_WALL;
                    }
                    break;
                case BorderWall.Vert:
                    tileX = wallIndexX * (BORDER_WALL_LENGTH + 1);
                    tileY = wallIndexY * (BORDER_WALL_LENGTH + 1) + 1;
                    borderVertColors[wallIndexX, wallIndexY] = Color.Lerp(Color.White, getTileColor(tileX, tileY), COLOR_TINT_FACTOR);
                    for (int j = 0; j < BORDER_WALL_LENGTH; j++)
                    {
                        grid[tileX, tileY + j] = LevelContent.LevelTile.Wall;
                        pathfinding.costGrid[tileX, tileY + j] = Pathfinding.COST_WALL;
                    }
                    break;
            }
        }

        // Every level creation does this
        private void initializeLevel(int x, int y)
        {
            pathfinding = new AStarPathfinding(GRID_SIZE, GRID_SIZE);
            xPos = x;
            yPos = y;
            alive = true;
            levelOverlayTexture = new Texture2D(RetroGame.graphicsDevice, TEX_SIZE, TEX_SIZE);
            collectableLocations = new List<int[]>();
            enemyLocations = new List<int[]>();
            prisonerLocations = new List<int[]>();
            powerupLocations = new List<int[]>();
        }

        // Create level from full fragment
        // _ _
        //|   |
        //|_ _|
        private void levelFull(LevelFragment fullFragment, int x, int y)
        {
            //initialize separately from fragmented levels because making a full level is simpler
            pathfinding = new AStarPathfinding(GRID_SIZE, GRID_SIZE);
            xPos = x;
            yPos = y;
            alive = true;

            levelOverlayTexture = new Texture2D(RetroGame.graphicsDevice, TEX_SIZE, TEX_SIZE);
            Color[] data = new Color[TEX_SIZE * TEX_SIZE];
            fragmentGrid[0, 0] = fullFragment;   fragmentGrid[0, 1] = fullFragment;
            fragmentGrid[1, 0] = fullFragment;   fragmentGrid[1, 1] = fullFragment;

            grid = (LevelContent.LevelTile[,])fullFragment.grid.Clone();
            grid = new LevelContent.LevelTile[GRID_SIZE, GRID_SIZE];
            for (int i = 0; i < GRID_SIZE; i++)
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    grid[i, j] = fullFragment.grid[i, j]; // cut off last row and column of fragment
                }

            collectableLocations = fullFragment.collectableLocations;
            enemyLocations = fullFragment.enemyLocations;
            prisonerLocations = fullFragment.prisonerLocations;
            powerupLocations = fullFragment.powerupLocations;

            finalizeLevel();
        }

        // _ _
        //| | |
        //|_|_|
        private void levelTwoVert(LevelFragment leftFragment, LevelFragment rightFragment, int x, int y)
        {
            if (leftFragment.type != LevelContent.Type.HalfVertical)
                throw new ArgumentException("Both fragments must be vertical halves", "leftFragment");
            if (rightFragment.type != LevelContent.Type.HalfVertical)
                throw new ArgumentException("Both fragments must be vertical halves", "rightFragment");

            initializeLevel(x, y);

            setFragmentIntoLevel(leftFragment, FragmentPosition.LeftHalf);
            setFragmentIntoLevel(rightFragment, FragmentPosition.RightHalf);

            setWallIntoLevel(BorderWall.Vert, 1, 0);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 1);

            finalizeLevel();
        }

        // _ _
        //| |_|
        //|_|_|
        private void levelLeftVertTwoCorner(LevelFragment leftFragment, LevelFragment topRightFragment, LevelFragment bottomRightFragment, int x, int y)
        {
            if (leftFragment.type != LevelContent.Type.HalfVertical)
                throw new ArgumentException("Left fragment must be a vertical half", "leftFragment");
            if (topRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both right fragments must be corners", "topRightFragment");
            if (bottomRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both right fragments must be corners", "bottomRightFragment");

            initializeLevel(x, y);

            setFragmentIntoLevel(leftFragment, FragmentPosition.LeftHalf);
            setFragmentIntoLevel(topRightFragment, FragmentPosition.TopRightCorner);
            setFragmentIntoLevel(bottomRightFragment, FragmentPosition.BottomRightCorner);

            setWallIntoLevel(BorderWall.Vert, 1, 0);
            setWallIntoLevel(BorderWall.Horiz, 1, 1);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 1);

            finalizeLevel();
        }

        // _ _
        //|_| |
        //|_|_|
        private void levelRightVertTwoCorner(LevelFragment topLeftFragment, LevelFragment bottomLeftFragment, LevelFragment rightFragment, int x, int y)
        {
            if (rightFragment.type != LevelContent.Type.HalfVertical)
                throw new ArgumentException("Right fragment must be a vertical half", "rightFragment");
            if (topLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both left fragments must be corners", "topLeftFragment");
            if (bottomLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both left fragments must be corners", "bottomLeftFragment");
            initializeLevel(x, y);

            setFragmentIntoLevel(topLeftFragment, FragmentPosition.TopLeftCorner);
            setFragmentIntoLevel(bottomLeftFragment, FragmentPosition.BottomLeftCorner);
            setFragmentIntoLevel(rightFragment, FragmentPosition.RightHalf);

            setWallIntoLevel(BorderWall.Vert, 1, 0);
            setWallIntoLevel(BorderWall.Horiz, 0, 1);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 1);

            finalizeLevel();
        }

        // _ _
        //|___|
        //|_ _|
        private void levelTwoHoriz(LevelFragment topFragment, LevelFragment bottomFragment, int x, int y)
        {
            if (topFragment.type != LevelContent.Type.HalfHorizontal)
                throw new ArgumentException("Both fragments must be horizontal halves", "topFragment");
            if (bottomFragment.type != LevelContent.Type.HalfHorizontal)
                throw new ArgumentException("Both fragments must be horizontal halves", "bottomFragment");
            initializeLevel(x, y);

            setFragmentIntoLevel(topFragment, FragmentPosition.TopHalf);
            setFragmentIntoLevel(bottomFragment, FragmentPosition.BottomHalf);

            setWallIntoLevel(BorderWall.Horiz, 0, 1);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Horiz, 1, 1);

            finalizeLevel();
        }

        // _ _
        //|_|_|
        //|_ _|
        private void levelBottomHorizTwoCorner(LevelFragment topLeftFragment, LevelFragment topRightFragment, LevelFragment bottomFragment, int x, int y)
        {
            if (bottomFragment.type != LevelContent.Type.HalfHorizontal)
                throw new ArgumentException("Bottom fragment must be a horizontal half", "bottomFragment");
            if (topLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both top fragments must be corners", "topLeftFragment");
            if (topRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both top fragments must be corners", "topRightFragment");
            initializeLevel(x, y);

            setFragmentIntoLevel(topLeftFragment, FragmentPosition.TopLeftCorner);
            setFragmentIntoLevel(topRightFragment, FragmentPosition.TopRightCorner);
            setFragmentIntoLevel(bottomFragment, FragmentPosition.BottomHalf);

            setWallIntoLevel(BorderWall.Vert, 1, 0);
            setWallIntoLevel(BorderWall.Horiz, 0, 1);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Horiz, 1, 1);

            finalizeLevel();
        }

        // _ _
        //|___|
        //|_|_|
        private void levelTopHorizTwoCorner(LevelFragment topFragment, LevelFragment bottomLeftFragment, LevelFragment bottomRightFragment, int x, int y)
        {
            if (topFragment.type != LevelContent.Type.HalfHorizontal)
                throw new ArgumentException("Top fragment must be a horizontal half", "topFragment");
            if (bottomLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both bottom fragments must be corners", "bottomLeftFragment");
            if (bottomRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("Both bottom fragments must be corners", "bottomRightFragment");
            initializeLevel(x, y);

            setFragmentIntoLevel(topFragment, FragmentPosition.TopHalf);
            setFragmentIntoLevel(bottomLeftFragment, FragmentPosition.BottomLeftCorner);
            setFragmentIntoLevel(bottomRightFragment, FragmentPosition.BottomRightCorner);

            setWallIntoLevel(BorderWall.Horiz, 0, 1);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Horiz, 1, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 1);

            finalizeLevel();
        }

        // _ _
        //|_|_|
        //|_|_|
        private void levelFourCorner(LevelFragment topLeftFragment, LevelFragment topRightFragment, LevelFragment bottomLeftFragment, LevelFragment bottomRightFragment, int x, int y)
        {
            if (topLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("All fragments must be corners", "topLeftFragment");
            if (topRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("All fragments must be corners", "topRightFragment");
            if (bottomLeftFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("All fragments must be corners", "bottomLeftFragment");
            if (bottomRightFragment.type != LevelContent.Type.Corner)
                throw new ArgumentException("All fragments must be corners", "bottomRightFragment");
            initializeLevel(x, y);

            setFragmentIntoLevel(topLeftFragment, FragmentPosition.TopLeftCorner);
            setFragmentIntoLevel(topRightFragment, FragmentPosition.TopRightCorner);
            setFragmentIntoLevel(bottomLeftFragment, FragmentPosition.BottomLeftCorner);
            setFragmentIntoLevel(bottomRightFragment, FragmentPosition.BottomRightCorner);

            setWallIntoLevel(BorderWall.Horiz, 0, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 0);
            setWallIntoLevel(BorderWall.Single, 1, 1);
            setWallIntoLevel(BorderWall.Horiz, 1, 1);
            setWallIntoLevel(BorderWall.Vert, 1, 1);

            finalizeLevel();
        }

        // Every level creation does this
        private void finalizeLevel()
        {
            foreach (int[] loc in collectableLocations)
                collectables.Add(new Gem(Level.TEX_SIZE * xPos + loc[0] * Level.TILE_SIZE + Level.TILE_SIZE / 2, Level.TEX_SIZE * yPos + loc[1] * Level.TILE_SIZE + Level.TILE_SIZE / 2, xPos, yPos, loc[0], loc[1]));
            foreach (int[] loc in enemyLocations)
                addEnemy(loc[0], loc[1], (int)RetroGame.rand.Next(4));
            foreach (int[] loc in prisonerLocations)
                prisoners.Add(levelManager.newPrisoner(loc[0], loc[1], new Color(RetroGame.rand.Next(255), RetroGame.rand.Next(255), RetroGame.rand.Next(255), 255), this));
            foreach (int[] loc in powerupLocations)
                powerups.Add(levelManager.newPowerup(loc[0], loc[1], this));

            cellName = GetCellName(xPos, yPos, alphabetOffset1, alphabetOffset2);

            resetPathfinding();
        }
        #endregion

        public void UpdateEscape(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
                enemy.Update(gameTime);
            if (prisoners.Count > 1)
                prisoners.Sort(new Comparison<Prisoner>((p1, p2) => { if (p1.collectedTime == p2.collectedTime) return 0; else if (p1.collectedTime > p2.collectedTime) return 1; else return -1; }));
            foreach (Prisoner p in prisoners)
                p.Update(gameTime);
            foreach (Collectable c in collectables)
                c.Update(gameTime);
            foreach (PowerupIcon p in powerups)
                p.Update(gameTime);

        }

        public void UpdateRetro(GameTime gameTime)
        {
            if (prisoners.Count > 1)
                prisoners.Sort(new Comparison<Prisoner>((p1, p2) => { if (p1.collectedTime == p2.collectedTime) return 0; else if (p1.collectedTime > p2.collectedTime) return 1; else return -1; }));
            foreach (Prisoner p in prisoners)
                p.Update(gameTime);
            foreach (Collectable c in collectables)
                c.Update(gameTime);
            foreach (PowerupIcon p in powerups)
                p.Update(gameTime);

        }

        public static bool tileWithinBounds(int tileX, int tileY)
        {
            return (tileX >= 0 && tileX < Level.GRID_SIZE) && (tileY >= 0 && tileY < Level.GRID_SIZE);
        }

        public bool collidesWithAnything(Vector2 position, Entity entity)
        {
            return collidesWithWall(position) || collidesWithEnemyGrid(position, entity);
        }

        public bool collidesWithWall(Vector2 position)
        {
            return levelManager.collidesWithWall(position);
        }

        public bool collidesWithEnemyGrid(Vector2 position, Entity entity)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            if (x <= 0 || y <= 0)
                return true;

            int tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            int tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;

            if (enemyGrid[tileX, tileY] != null && enemyGrid[tileX, tileY] != entity)
                return true;

            return false;
        }

        public Texture2D getOverlayTexture()
        {
            return levelOverlayTexture;
        }

        public Texture2D getTexture(FragmentPosition position)
        {
            switch (position)
            {
                case FragmentPosition.TopLeftCorner:
                case FragmentPosition.TopHalf:
                case FragmentPosition.LeftHalf:
                    return fragmentGrid[0, 0].levelTexture;
                case FragmentPosition.TopRightCorner:
                case FragmentPosition.RightHalf:
                    return fragmentGrid[1, 0].levelTexture;
                case FragmentPosition.BottomLeftCorner:
                case FragmentPosition.BottomHalf:
                    return fragmentGrid[0, 1].levelTexture;
                case FragmentPosition.BottomRightCorner:
                    return fragmentGrid[1, 1].levelTexture;
                default:
                    return null;
            }
        }

        public Texture2D getDebugTexture(FragmentPosition position)
        {
            switch (position)
            {
                case FragmentPosition.TopLeftCorner:
                case FragmentPosition.TopHalf:
                case FragmentPosition.LeftHalf:
                    return fragmentGrid[0, 0].levelTextureDebug;
                case FragmentPosition.TopRightCorner:
                case FragmentPosition.RightHalf:
                    return fragmentGrid[1, 0].levelTextureDebug;
                case FragmentPosition.BottomLeftCorner:
                case FragmentPosition.BottomHalf:
                    return fragmentGrid[0, 1].levelTextureDebug;
                case FragmentPosition.BottomRightCorner:
                    return fragmentGrid[1, 1].levelTextureDebug;
                default:
                    return null;
            }
        }

        public void addEnemy(int tileX, int tileY, int type, bool forceSandToSpawn = false)
        {
            foreach (Enemy e in enemies)
            {
                if (e.tileX == tileX && e.tileY == tileY)
                    levelManager.enemiesToRemove.Add(e);
            }
            enemies.Add(new Enemy(tileX, tileY, type, this, forceSandToSpawn));
        }

        public void removePrisoner(Prisoner p)
        {
            prisoners.Remove(p);
        }

        public void removePowerup(PowerupIcon p)
        {
            powerups.Remove(p);
        }

        public Color getTileColor(int tileX, int tileY)
        {
            Color tileColor = Color.White;
            int xFragment = tileX / (LevelContent.LEVEL_SIZE_HALF);
            int yFragment = tileY / (LevelContent.LEVEL_SIZE_HALF);
            if (tileX == 0 || tileY == 0)
            {
                if (xPos == 0 || yPos == 0)
                    tileColor = Color.White;
                else if (tileX == 0 && tileY == 0) // tile at topleft corner between levels
                {
                    Color leftColor = RetroGame.getLevels()[xPos - 1, yPos].getTileColor(GRID_SIZE - 1, tileY);
                    Color topColor = RetroGame.getLevels()[xPos, yPos - 1].getTileColor(tileX, GRID_SIZE - 1);
                    Color cornerColor = RetroGame.getLevels()[xPos - 1, yPos - 1].getTileColor(GRID_SIZE - 1, GRID_SIZE - 1);
                    tileColor = Color.Lerp(Color.Lerp(leftColor, topColor, 0.5f), Color.Lerp(cornerColor, getTileColor(1, 1), 0.5f), 0.5f);
                }
                else if (tileX == 0) // tile in between levels on left
                {
                    tileColor = Color.Lerp(RetroGame.getLevels()[xPos - 1, yPos].getTileColor(GRID_SIZE - 1, tileY), getTileColor(1, tileY), 0.5f);
                }
                else if (tileY == 0) // tile in between levels on top
                {
                    tileColor = Color.Lerp(RetroGame.getLevels()[xPos, yPos - 1].getTileColor(tileX, GRID_SIZE - 1), getTileColor(tileX, 1), 0.5f);
                }
            }
            else if (tileX == LevelContent.LEVEL_SIZE_HALF - 1 && tileY == LevelContent.LEVEL_SIZE_HALF - 1) // tile at center of 4 fragments
            {
                tileColor = Color.Lerp(Color.Lerp(fragmentGrid[0, 0].color, fragmentGrid[1, 0].color, 0.5f),
                                        Color.Lerp(fragmentGrid[0, 1].color, fragmentGrid[1, 1].color, 0.5f), 0.5f);
            }
            else if (tileX == LevelContent.LEVEL_SIZE_HALF - 1) // tile in between fragments
            {
                tileColor = Color.Lerp(fragmentGrid[0, yFragment].color, fragmentGrid[1, yFragment].color, 0.5f);
            }
            else if (tileY == LevelContent.LEVEL_SIZE_HALF - 1) // tile in between fragments
            {
                tileColor = Color.Lerp(fragmentGrid[xFragment, 0].color, fragmentGrid[xFragment, 1].color, 0.5f);
            }
            else
            { 
                tileColor = fragmentGrid[xFragment, yFragment].color;
            }
            return tileColor;
        }

        public void updateLeftBorderColors()
        {
            setWallIntoLevel(BorderWall.Vert, 0, 0);
            setWallIntoLevel(BorderWall.Single, 0, 1);
            setWallIntoLevel(BorderWall.Vert, 0, 1);
        }

        public void updateTopBorderColors()
        {
            setWallIntoLevel(BorderWall.Horiz, 0, 0);
            setWallIntoLevel(BorderWall.Single, 1, 0);
            setWallIntoLevel(BorderWall.Horiz, 1, 0);
        }

        public void updateCornerBorderColors()
        {
            setWallIntoLevel(BorderWall.Single, 0, 0);
        }

        public void resetPathfinding()
        {
            pathfinding.Reset();
            for (int i = 0; i < grid.GetLength(0); i++)
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] == LevelContent.LevelTile.Wall)
                        pathfinding.costGrid[i, j] = Pathfinding.COST_WALL;
                    else
                        pathfinding.costGrid[i, j] = Pathfinding.COST_FLOOR;
                }
        }

        public virtual bool drillWall(int tileX, int tileY)
        {
            try
            {
                if (grid[tileX, tileY] != LevelContent.LevelTile.Wall)
                    return true;
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }

            if (!drilledWalls.Contains(new Point(tileX, tileY)))
                drilledWalls.Add(new Point(tileX, tileY));
            grid[tileX, tileY] = LevelContent.LevelTile.Floor;
            pathfinding.costGrid[tileX, tileY] = Pathfinding.COST_FLOOR;
            resetPathfinding();

            Color tileColor = getTileColor(tileX, tileY);
            Color[] tileData = new Color[TILE_SIZE * TILE_SIZE];
            texFloor.GetData<Color>(tileData);
            for (int d = 0; d < tileData.Length; d++)
                tileData[d] = tileData[d].Tint(tileColor, WALL_TINT_FACTOR);
            levelOverlayTexture.SetData<Color>(0, new Rectangle(tileX * TILE_SIZE, tileY * TILE_SIZE, TILE_SIZE, TILE_SIZE), tileData, 0, TILE_SIZE * TILE_SIZE);

            double chance = RetroGame.rand.NextDouble();
            if (chance < CHANCE_TO_SPAWN_SAND_ON_DRILL)
            {
                int i = tileX;
                int j = tileY;
                collectables.Add(new Sand(TEX_SIZE * xPos + i * TILE_SIZE + 16, TEX_SIZE * yPos + j * TILE_SIZE + 16, xPos, yPos, i, j));
            }
            RetroGame.AddScore(DRILL_WALL_SCORE);
            RetroGame.HasDrilled = true;
            return true;
        }

        public void undrillWall(int tileX, int tileY)
        {
            drilledWalls.Remove(new Point(tileX, tileY));
            grid[tileX, tileY] = LevelContent.LevelTile.Wall;
            pathfinding.costGrid[tileX, tileY] = Pathfinding.COST_WALL;
            resetPathfinding();

            Color[] tileData = new Color[TILE_SIZE * TILE_SIZE];
            levelOverlayTexture.SetData<Color>(0, new Rectangle(tileX * TILE_SIZE, tileY * TILE_SIZE, TILE_SIZE, TILE_SIZE), tileData, 0, TILE_SIZE * TILE_SIZE);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Vector2 baseOffset = new Vector2(xPos * Level.TEX_SIZE, yPos * Level.TEX_SIZE);
            bool[,] fragmentAlreadyDrawn = new bool[2, 2];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    Vector2 fragmentOffset = new Vector2(i * Level.TILE_SIZE * (LevelContent.LEVEL_SIZE_HALF - 1), j * Level.TILE_SIZE * (LevelContent.LEVEL_SIZE_HALF - 1));
                    LevelFragment fragment = fragmentGrid[i, j];
                    if (!fragmentAlreadyDrawn[i, j])
                    {
                        fragment.Draw(spriteBatch, baseOffset + fragmentOffset);
                        switch (fragment.type)
                        {
                            case LevelContent.Type.Full:
                                fragmentAlreadyDrawn[0, 0] = fragmentAlreadyDrawn[0, 1] = fragmentAlreadyDrawn[1, 0] = fragmentAlreadyDrawn[1, 1] = true;
                                break;
                            case LevelContent.Type.HalfHorizontal:
                                fragmentAlreadyDrawn[0, j] = fragmentAlreadyDrawn[1, j] = true;
                                break;
                            case LevelContent.Type.HalfVertical:
                                fragmentAlreadyDrawn[i, 0] = fragmentAlreadyDrawn[i, 1] = true;
                                break;
                            case LevelContent.Type.Corner:
                                fragmentAlreadyDrawn[i, j] = true;
                                break;
                        }
                    }
                }
            //Draw border walls
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    int tileX = 0, tileY = 0;
                    if (borderCornerColors[i, j] != null)
                    {
                        tileX = i * (BORDER_WALL_LENGTH + 1);
                        tileY = j * (BORDER_WALL_LENGTH + 1);
                        spriteBatch.Draw(texWall, baseOffset + new Vector2(tileX * TILE_SIZE, tileY * TILE_SIZE), null, borderCornerColors[i, j].Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                    }
                    if (borderHorizColors[i, j] != null)
                    {
                        tileX = i * (BORDER_WALL_LENGTH + 1) + 1;
                        tileY = j * (BORDER_WALL_LENGTH + 1);
                        spriteBatch.Draw(texWallHoriz, baseOffset + new Vector2(tileX * TILE_SIZE, tileY * TILE_SIZE), null, borderHorizColors[i, j].Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                    }
                    if (borderVertColors[i, j] != null)
                    {
                        tileX = i * (BORDER_WALL_LENGTH + 1);
                        tileY = j * (BORDER_WALL_LENGTH + 1) + 1;
                        spriteBatch.Draw(texWallVert, baseOffset + new Vector2(tileX * TILE_SIZE, tileY * TILE_SIZE), null, borderVertColors[i, j].Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                    }
                }
            //Draw overlay last for drilled walls and such
            spriteBatch.Draw(levelOverlayTexture, baseOffset, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);

            foreach (Collectable c in collectables)
                if (c != null)
                    c.Draw(spriteBatch);
            foreach (Enemy enemy in enemies)
                enemy.Draw(spriteBatch);
            foreach (Prisoner p in prisoners)
                p.Draw(spriteBatch);
            foreach (PowerupIcon p in powerups)
                p.Draw(spriteBatch);
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            Vector2 baseOffset = new Vector2(xPos * TEX_SIZE, yPos * TEX_SIZE);
            bool[,] fragmentAlreadyDrawn = new bool[2, 2];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    Vector2 fragmentOffset = new Vector2(i * TILE_SIZE * (LevelContent.LEVEL_SIZE_HALF -1 ), j * TILE_SIZE * (LevelContent.LEVEL_SIZE_HALF - 1));
                    LevelFragment fragment = fragmentGrid[i, j];
                    if (!fragmentAlreadyDrawn[i,j])
                    {
                        spriteBatch.Draw(fragment.levelTextureDebug, baseOffset + fragmentOffset, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                        switch (fragment.type)
                        {
                            case LevelContent.Type.Full:
                                fragmentAlreadyDrawn[0, 0] = fragmentAlreadyDrawn[0, 1] = fragmentAlreadyDrawn[1, 0] = fragmentAlreadyDrawn[1, 1] = true;
                                break;
                            case LevelContent.Type.HalfHorizontal:
                                fragmentAlreadyDrawn[0, j] = fragmentAlreadyDrawn[1, j] = true;
                                break;
                            case LevelContent.Type.HalfVertical:
                                fragmentAlreadyDrawn[i, 0] = fragmentAlreadyDrawn[i, 1] = true;
                                break;
                            case LevelContent.Type.Corner:
                                fragmentAlreadyDrawn[i, j] = true;
                                break;
                        }
                    }
                }
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new LevelMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class LevelMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            List<Point> drilledWalls;
            Enemy[,] enemyGrid;
            IMemento[] enemyMementos;

            public LevelMemento(Level target)
            {
                //save necessary information from target here
                Target = target;
                drilledWalls = new List<Point>(target.drilledWalls);
                enemyGrid = (Enemy[,])target.enemyGrid.Clone();
                enemyMementos = new IMemento[target.enemies.Count];
                for (int i = 0; i < enemyMementos.Length; i++)
                {
                    enemyMementos[i] = target.enemies[i].GenerateMementoFromCurrentFrame();
                }
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Level target = (Level)Target;
                if (isNewFrame) //only apply frame if it's for the first time, ignore intermediate interpolations
                {
                    target.enemyGrid = enemyGrid;
                    for (int i = 0; i < target.drilledWalls.Count; i++)
                    {
                        Point p = target.drilledWalls[i];
                        if (!drilledWalls.Contains(p))
                        {
                            target.undrillWall(p.X, p.Y);
                            i--;
                        }
                    }
                }

                for (int i = 0; i < enemyMementos.Length; i++)
                {
                    IMemento nextEnemyFrame = null;
                    if (nextFrame != null)
                    {
                        nextEnemyFrame = ((LevelMemento)nextFrame).enemyMementos[i];
                    }
                    enemyMementos[i].Apply(interpolationFactor, isNewFrame, nextEnemyFrame);
                }
            }
        }

        ~Level() //apparently you can do this in C#... called before garbage collection
        {
            levelOverlayTexture.Dispose();
        }
    }
}
