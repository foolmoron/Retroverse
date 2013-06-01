using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Particles;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Retroverse
{
    public class LevelManager
    {
        public static readonly int MAX_LEVELS = 500;
        public static readonly Point STARTING_LEVEL = new Point(1, MAX_LEVELS / 2);
        public static readonly Point STARTING_TILE = new Point(15, 15);

        public Hero[] heroes;
        public readonly Level[,] levels = new Level[MAX_LEVELS, MAX_LEVELS];
        private List<Level> currentLevels = null;
        public List<Level> CurrentLevels
        {
            get
            {
                if (currentLevels == null)
                {
                    currentLevels = new List<Level>();
                    foreach (Hero hero in heroes)
                        for (int xi = -1; xi <= 1; xi++)
                            for (int yi = -1; yi <= 1; yi++)
                            {
                                int x = hero.levelX + xi;
                                int y = hero.levelY + yi;
                                if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS || levels[x, y] == null || currentLevels.Contains(levels[x,y]))
                                    continue;
                                currentLevels.Add(levels[x, y]);
                            }
                    currentLevels.Sort(new Comparison<Level>((level1, level2) => (level1.xPos + (level1.yPos * MAX_LEVELS)) - (level2.xPos + (level2.yPos * MAX_LEVELS))));
                }
                return currentLevels;
            }
        }
        public Dictionary<Hero, List<Level>> LevelsSurroundingHero = new Dictionary<Hero, List<Level>>();

        // entities to remove on next frame
        public List<Collectable> collectablesToRemove = new List<Collectable>();
        public List<Enemy> enemiesToRemove = new List<Enemy>();

        public Camera Camera { get; set; }
        public CameraMode CameraMode { get; private set; }

        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch staticSpriteBatch;
        public static RenderTarget2D levelBorderVertStaticTarget;
        public static RenderTarget2D levelBorderHorizStaticTarget;
        public static readonly Point VERT_STATIC_DIMENSIONS = new Point(Level.TILE_SIZE / 2, Level.TEX_SIZE);
        public static readonly Point HORIZ_STATIC_DIMENSIONS = new Point( Level.TEX_SIZE, Level.TILE_SIZE / 2);

        public static void Load(GraphicsDevice graphics)
        {
            graphicsDevice = graphics;
            staticSpriteBatch = new SpriteBatch(graphics);
            levelBorderVertStaticTarget = new RenderTarget2D(graphics, VERT_STATIC_DIMENSIONS.X, VERT_STATIC_DIMENSIONS.Y);
            levelBorderHorizStaticTarget = new RenderTarget2D(graphics, HORIZ_STATIC_DIMENSIONS.X, HORIZ_STATIC_DIMENSIONS.Y);
        }

        public LevelManager()
        {
            heroes = new Hero[RetroGame.MAX_PLAYERS];
            for (int i = 0; i < heroes.Length; i++)
            {
                heroes[i] = new Hero((PlayerIndex)Enum.Parse(typeof(PlayerIndex), i.ToString()));
            }
        }

        public void Initialize(int numPlayers, bool createNewHeroes, LevelFragment fullIntroLevelFragment, Point startingLevel)
        {
            if (numPlayers <= 0 || numPlayers > RetroGame.MAX_PLAYERS)
                throw new ArgumentOutOfRangeException("numPlayers", "Number of players must be between 1 and " + RetroGame.MAX_PLAYERS);
            if (fullIntroLevelFragment.type != LevelContent.Type.Full)
                throw new ArgumentException("LevelManager must be initialized with a Full LevelFragment", "fullIntroLevelFragment");
            if (createNewHeroes){
                heroes = new Hero[numPlayers];
                for (int i = 0; i < heroes.Length; i++)
                {
                    heroes[i] = new Hero((PlayerIndex)Enum.Parse(typeof(PlayerIndex), i.ToString()));
                    heroes[i].Initialize();
                }
            }
            createSpecificLevelAt(fullIntroLevelFragment, startingLevel.X, startingLevel.Y);
            if (fullIntroLevelFragment.type == LevelContent.Type.Full)
            {
                for(int i = 0; i < numPlayers; i++)
                {
                    for (int j = 0; j < fullIntroLevelFragment.specialTiles[i].Length; j++)
                    {
                        if (fullIntroLevelFragment.specialTiles[i][j] != null)
                            levels[startingLevel.X, startingLevel.Y].powerups.Add(newPowerup(Powerups.SPECIAL_INTRO_POWERUPS[j],
                                                                    fullIntroLevelFragment.specialTiles[i][j][0], fullIntroLevelFragment.specialTiles[i][j][1],
                                                                    levels[startingLevel.X, startingLevel.Y]));
                    }
                    if (fullIntroLevelFragment.heroTiles[0] != null && heroes.Length >= 1)
                        heroes[i].position = new Vector2(Level.TEX_SIZE * startingLevel.X +
                                                         (Level.TILE_SIZE * (fullIntroLevelFragment.heroTiles[i][0] + 0.5f)),
                                                         Level.TEX_SIZE * startingLevel.Y +
                                                         (Level.TILE_SIZE * (fullIntroLevelFragment.heroTiles[i][1] + 0.5f)));
                }
            }
            foreach(Hero hero in heroes)
            {
                hero.updateCurrentLevelAndTile();
            }
            SetCameraMode(CameraMode.Arena);
        }

        public void removeLevel(int xPos, int yPos)
        {
            if (levels[xPos, yPos] != null)
                levels[xPos, yPos].alive = false;
            levels[xPos, yPos] = null;
            currentLevels = null; // make the CurrentLevels list be recalculated
        }

        public void addEnemy(int x, int y, int type, Level l, bool forceSandToSpawn = false)
        {
            l.addEnemy(x, y, type, forceSandToSpawn);
        }

        public Prisoner newPrisoner(int tileX, int tileY, Color c, Level l)
        {
            return new Prisoner(c, Names.getRandomName(), l.xPos * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.xPos, l.yPos, tileX, tileY);
        }

        public PowerupIcon newPowerup(int tileX, int tileY, Level l)
        {
            return new PowerupIcon(l.xPos * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.xPos, l.yPos, tileX, tileY, Powerups.RandomWildPowerupType());
        }

        public PowerupIcon newPowerup(Type powerupType, int tileX, int tileY, Level l)
        {
            return new PowerupIcon(l.xPos * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.xPos, l.yPos, tileX, tileY, powerupType);
        }

        public PowerupIcon newRegeneratingPowerup(Type powerupType, int tileX, int tileY, Level l)
        {
            return new PowerupIconRegenerating(l.xPos * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2, l.xPos, l.yPos, tileX, tileY, powerupType);
        }

        public Matrix getTranslation()
        {
            return Camera.GetTranslation();
        }

        public Matrix getScale()
        {
            return Camera.GetScale();
        }

        public Matrix getViewMatrix()
        {
            return Camera.GetViewMatrix();
        }

        public void SetCameraMode(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.Arena:
                    ArenaCamera arenaCamera = new ArenaCamera(new Vector2((RetroGame.getMainLiveHero().levelX + 0.5f) * Level.TEX_SIZE + Level.TILE_SIZE / 2, (RetroGame.getMainLiveHero().levelY + 0.5f) * Level.TEX_SIZE + Level.TILE_SIZE / 2));
                    arenaCamera.Initialize();
                    if (Camera != null)
                        arenaCamera.InitializeWithCamera(Camera);
                    Camera = arenaCamera;
                    break;
                case CameraMode.Escape:
                    EscapeCamera escapeCamera;
                    if (RetroGame.NUM_PLAYERS == 1)
                        escapeCamera = new EscapeCamera(RetroGame.getMainLiveHero());
                    else //if (RetroGame.NUM_PLAYERS == 2)
                    {
                        int liveHeroes = 0;
                        foreach(Hero hero in RetroGame.getHeroes())
                            if (hero.Alive)
                                liveHeroes++;
                        if (liveHeroes == 1)
                            escapeCamera = new EscapeCamera(RetroGame.getMainLiveHero());
                        else
                            escapeCamera = new CoopEscapeCamera(RetroGame.getHeroes()[0], RetroGame.getHeroes()[1]);
                    }
                    escapeCamera.Initialize();
                    if (Camera != null)
                        escapeCamera.InitializeWithCamera(Camera);
                    Camera = escapeCamera;
                    break;
            }
            CameraMode = mode;
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
            
            int levelX = x / Level.TEX_SIZE; // get which level you are going to
            int levelY = y / Level.TEX_SIZE;
            if (levelX >= MAX_LEVELS || levelY >= MAX_LEVELS)
                return false;
            Level level = RetroGame.getLevels()[levelX, levelY];
            if (level == null)
                return false;

            int tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            int tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;
            LevelContent.LevelTile tile = level.grid[tileX, tileY];

            if (tile == LevelContent.LevelTile.Wall)
                return false;
            else if (entity is Enemy && level.enemyGrid[tileX, tileY] != null && level.enemyGrid[tileX, tileY] != entity)
                return false;

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
                return true;
            Level level = RetroGame.getLevels()[levelX, levelY];
            if (level == null)
                return true;

            int tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            int tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;

            LevelContent.LevelTile tile = level.grid[tileX, tileY];            
            switch (tile)
            {
                case LevelContent.LevelTile.Wall:
                    return true;
                default:
                    break;
            }

            return false;
        }

        public void UpdateArena(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            Camera.Update(gameTime);
            UpdateEscape(gameTime);
        }

        public void UpdateEscape(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            //remove entities
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
                        else if (c is PowerupIcon)
                        {
                            l.removePowerup((PowerupIcon) c);
                        }
                        else
                        {
                            l.collectables.Remove(c);
                        }
                }
            }
            collectablesToRemove.Clear();
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

            foreach (Hero hero in heroes)
                hero.Update(gameTime);

            foreach (Level l in CurrentLevels)
                l.UpdateEscape(gameTime);

            bool levelChanged = false;
            foreach (Hero hero in heroes)
                levelChanged |= hero.levelChanged;
            if (levelChanged)
            {
                createAndRemoveLevels();
                if(RetroGame.State == GameState.Arena)
                    RetroGame.EnterEscapeMode();
            }

            float mult = 0f;
            float currentZoomSpeed = Camera.zoomSpeed;
#if DEBUG
            if (heroes[0].isDown(Keys.E))
                mult = 2.7f;
            else if (heroes[0].isDown(Keys.R))
                mult = -2.7f;
            Camera.targetZoom += Camera.zoomSpeed * mult * seconds;
#endif
            if (mult != 0f)
                currentZoomSpeed = 5 * Camera.zoomSpeed;

            if (Camera.targetZoom - Camera.zoom > seconds * currentZoomSpeed)
                Camera.zoom += currentZoomSpeed * seconds;
            else if (Camera.targetZoom - Camera.zoom < -seconds * currentZoomSpeed)
                Camera.zoom -= currentZoomSpeed * seconds;
            else
                Camera.zoom = Camera.targetZoom;
            Camera.Update(gameTime);
        }

        public void UpdateRetro(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
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
                            else if (c is PowerupIcon)
                            {
                                l.removePowerup((PowerupIcon)c);
                            }
                            else
                            {
                                l.collectables.Remove(c);
                            }
                    }
                }
            collectablesToRemove.Clear();
            // Update non-revertible collectables still
            foreach (Level l in CurrentLevels)
                l.UpdateRetro(gameTime);

            foreach (Hero hero in heroes)
                hero.updateCurrentLevelAndTile();
            createAndRemoveLevels();
            Camera.Update(gameTime);
        }

        public void createSpecificLevelAt(LevelFragment fullLevelFragment, int xPos, int yPos)
        {
            if (fullLevelFragment.type != LevelContent.Type.Full)
                throw new ArgumentException("Creating a specific level requires a full level fragment", "fullLevelFragment");
            levels[xPos, yPos] = new Level(this, fullLevelFragment, xPos, yPos);
            currentLevels = null; // make the CurrentLevels list be recalculated
        }

        public void putPremadeLevelAt(Level premadeLevel, int xPos, int yPos)
        {
            premadeLevel.levelManager = this;
            premadeLevel.xPos = xPos;
            premadeLevel.yPos = yPos;
            levels[xPos, yPos] = premadeLevel;
            currentLevels = null; // make the CurrentLevels list be recalculated
        }

        public void createRandomLevelAt(int xPos, int yPos)
        {
            levels[xPos, yPos] = new Level(this, xPos, yPos);
            currentLevels = null; // make the CurrentLevels list be recalculated
        }

        public void createRandomLevelAt(int xPos, int yPos, int levelLayout)
        {
            levels[xPos, yPos] = new Level(this, xPos, yPos, levelLayout);
            currentLevels = null; // make the CurrentLevels list be recalculated
        }

        public void createAndRemoveLevels()
        {
            List<Point> levelsWithHeroes = new List<Point>();
            foreach (Hero hero in heroes)
                levelsWithHeroes.Add(new Point(hero.levelX, hero.levelY));
            List<Point> levelsToHave = new List<Point>();
            List<Point> levelsToRemove = new List<Point>();
            foreach (Hero hero in heroes)
            {
                int prevHeroLevelX = hero.prevLevelX;
                int prevHeroLevelY = hero.prevLevelY;
                int curHeroLevelX = hero.levelX;
                int curHeroLevelY = hero.levelY;

                if (curHeroLevelX < prevHeroLevelX) // hero transitioned left
                {
                    int indexX = curHeroLevelX + 2;
                    int indexY = curHeroLevelY;
                    if (indexX < MAX_LEVELS)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            if (indexY + i < MAX_LEVELS && indexY + i >= 0)
                                levelsToRemove.Add(new Point(indexX, indexY + i));
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
                                levelsToRemove.Add(new Point(indexX, indexY + i));
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
                                levelsToRemove.Add(new Point(indexX + i, indexY));
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
                                levelsToRemove.Add(new Point(indexX + i, indexY));
                        }
                    }
                }

                List<Level> surroundingLevels = new List<Level>();
                for (int xi = -1; xi <= 1; xi++)
                    for (int yi = -1; yi <= 1; yi++)
                    {
                        int x = curHeroLevelX + xi;
                        int y = curHeroLevelY + yi;
                        if (x < 0 || y < 0 || x >= MAX_LEVELS || y >= MAX_LEVELS)
                            continue;
                        levelsToHave.Add(new Point(x, y));
                        surroundingLevels.Add(levels[x, y]);
                        //clear all registered levels in History
                        if (!levelsWithHeroes.Contains(new Point(x, y)) && History.IsRegistered(levels[x, y]))
                            History.UnRegisterReversible(levels[x, y]);
                    }
                LevelsSurroundingHero[hero] = surroundingLevels;
            }
            foreach (Point p in levelsToRemove)
                if (!levelsToHave.Contains(p))
                    removeLevel(p.X, p.Y);
            List<Point> levelsToUpdateBorderWallColors = new List<Point>();
            foreach (Point p in levelsToHave)
                if (levels[p.X, p.Y] == null)
                {
                    createRandomLevelAt(p.X, p.Y);
                    foreach (Point newP in new Point[] { p, new Point(p.X + 1, p.Y), new Point(p.X, p.Y + 1), new Point(p.X + 1, p.Y + 1) })
                        if (!levelsToUpdateBorderWallColors.Contains(newP))
                            levelsToUpdateBorderWallColors.Add(newP);
                }
            foreach (Point p in levelsToUpdateBorderWallColors)
                if (p.X > 0 && p.Y > 0 && levels[p.X, p.Y] != null)
                {
                    if (levels[p.X - 1, p.Y] != null)
                        levels[p.X, p.Y].updateLeftBorderColors();
                    if (levels[p.X, p.Y - 1] != null)
                        levels[p.X, p.Y].updateTopBorderColors();
                    if (levels[p.X - 1, p.Y] != null && levels[p.X, p.Y - 1] != null && levels[p.X - 1, p.Y - 1] != null)
                        levels[p.X, p.Y].updateCornerBorderColors();
                }
            foreach (Hero hero in heroes)
            {
                if (!History.IsRegistered(levels[hero.levelX, hero.levelY]))
                    History.RegisterReversible(levels[hero.levelX, hero.levelY]);
            }
        }

        public static void PreDraw()
        {
            Effect staticEffect = Effects.StaticWithAlpha;
            staticEffect.Parameters["randomSeed"].SetValue((float)RetroGame.rand.NextDouble());
            staticEffect.Parameters["whiteness"].SetValue(MenuScreen.STATIC_WHITENESS);
            staticEffect.Parameters["premultiply"].SetValue(true);

            graphicsDevice.SetRenderTarget(levelBorderVertStaticTarget);
            graphicsDevice.Clear(Color.Transparent);
            staticEffect.Parameters["AlphaTexture"].SetValue(TextureManager.Get("staticlevelbordervertalpha"));
            staticSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise,
            staticEffect, Matrix.Identity);
            staticSpriteBatch.Draw(RetroGame.PIXEL, new Rectangle(0, 0, levelBorderVertStaticTarget.Width, levelBorderVertStaticTarget.Height), Color.White);
            staticSpriteBatch.End();

            graphicsDevice.SetRenderTarget(levelBorderHorizStaticTarget);
            graphicsDevice.Clear(Color.Transparent);
            staticEffect.Parameters["AlphaTexture"].SetValue(TextureManager.Get("staticlevelborderhorizalpha"));
            staticSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise,
            staticEffect, Matrix.Identity);
            staticSpriteBatch.Draw(RetroGame.PIXEL, new Rectangle(0, 0, levelBorderHorizStaticTarget.Width, levelBorderHorizStaticTarget.Height), Color.White);
            staticSpriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Level l in CurrentLevels)
                l.Draw(spriteBatch);

            for(int i = heroes.Length - 1; i >= 0; i--)
            {
                Hero hero = heroes[i];
                hero.Draw(spriteBatch);
                if (RetroGame.DEBUG)
                    hero.DrawDebug(spriteBatch);
            }
            
            foreach (Level l in CurrentLevels)
            {
                spriteBatch.Draw(levelBorderVertStaticTarget, new Vector2(l.xPos * Level.TEX_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + Level.TILE_SIZE / 2), null, Color.White, 0, new Vector2(levelBorderVertStaticTarget.Width / 2.0f, 0), 1, SpriteEffects.None, 0);
                spriteBatch.Draw(levelBorderHorizStaticTarget, new Vector2(l.xPos * Level.TEX_SIZE + Level.TILE_SIZE / 2, l.yPos * Level.TEX_SIZE + Level.TILE_SIZE / 2), null, Color.White, 0, new Vector2(0, levelBorderHorizStaticTarget.Height / 2.0f), 1, SpriteEffects.None, 0);
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

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            foreach (Level l in CurrentLevels)
                l.DrawDebug(spriteBatch);
        }

        public void DrawDebugHUD(SpriteBatch spriteBatch)
        {
        }
    }
}