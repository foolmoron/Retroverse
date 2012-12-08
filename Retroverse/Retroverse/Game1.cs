using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Windows.Forms;
using Particles;
using System.IO;

namespace Retroverse
{
    public enum GameState { Arena, Escape, RetroPort, StartScreen, PauseScreen, GameOverScreen };
    public enum ScreenSize { Small, Medium, Large, Huge };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        public static Game1 game;
        public static readonly bool TEST_INTRO_ARENA = true || !DEBUG;
        public static readonly bool INVINCIBILITY = true && DEBUG;

        public static readonly Dictionary<string, Color> LEVEL_COLORS = new Dictionary<string, Color>()
        {
            {"intro", new Color(69,104,165)},
            {"rachel1", new Color(76,40,156)},
            {"rachel2", new Color(228,188,0)},
            {"rachel3", new Color(252,72,0)},
            {"rachel4", new Color(102,205,170)},
        };

        public static Random rand = new Random();
        public static GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public SpriteBatch spriteBatchHUD;
        public RenderTarget2D shaderRenderTarget;
        public static Effect currentEffect = null;

        //DRAW OPTIONS
        public static bool drawLevelDebugTextures =  true; // draw colored squares that back the levels
        public static bool drawVignette = false; // draw pulsing colored vignette
        public static bool drawEffects = false; // draw pulsing black and white effect

        public static ScreenSize currentScreenSizeMode = ScreenSize.Small;
        public static readonly int[] SCREEN_SIZES = new int[] { 600, 700, 800, 900 };
        public static readonly int[] HUD_SIZES = new int[] { 51, 66, 82, 101 };
        public static readonly int[] LEVEL_OFFSETS_FROM_HUD = new int[] { 80, 90, 100, 110 };
        public static int hudSize;
        public static int levelOffsetFromHUD;
        public static Vector2 screenSize;
        public static bool screenSizeChanged = false;
        public static Texture2D PIXEL;
        public static SpriteFont FONT_DEBUG;
        public static SpriteFont FONT_PIXEL_SMALL;
        public static SpriteFont FONT_HUD_KEYS;
        public static Viewport viewport;
        public static GameState state;
        public static bool retroStatisActive = false;
        public static float timeScale = 1f;
        public static GraphicsDevice graphicsDevice;
        public static Dictionary<string, Level> levelTemplates = new Dictionary<string, Level>();
        private static Level introLevelTemplate;

        // screens
        public static GameState lastState;
        public static SpriteFont FONT_PIXEL_LARGE;
        public static SpriteFont FONT_PIXEL_HUGE;
        public static string screenText;

        // fps counter
        public double framerate;
        public double frameTimeCounter;
        public int frameCounter;

        public static readonly float VIGNETTE_MAX_INTENSITY = 1.2f;
        public static readonly float VIGNETTE_MIN_INTENSITY = 0.0f;
        public static float vignetteIntensity = VIGNETTE_MIN_INTENSITY;
        public static float vignetteMultiplier = 1f;
        public static readonly float VIGNETTE_PULSE_SPEED = 1.2f;
        public static Color vignetteColor = Color.Red;

        public static readonly float EFFECT_RADIUS_MIN = 20f;
        public static readonly float EFFECT_RADIUS_MAX = 300f;
        public static readonly float EFFECT_RADIUS_SPEED = 250f;
        public static float effectRadiusMultiplier = 1;
        public static float effectRadius = EFFECT_RADIUS_MIN;

        // on-screen text excalamation
        public static SpriteFont FONT_EXCLAMATION;
        private static string[] exclamationStrings;
        private static Color[] exclamationColors;
        private static float exclamationDurationSecs;
        private static float currentExclamationTime;

        // score and sand management
        private static int score = 0;
        private static int lastScore = 0;
        public static readonly int MAX_SAND = 10;
        public static int availableSand { get; private set; }

        public static LevelManager levelManager = new LevelManager();
        Song Aragonaise;
        bool songstart = false;
        public static int checkpoint = 4;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            TargetElapsedTime = new TimeSpan(10000000L / 60L); // target fps
            if (TEST_INTRO_ARENA)
            {
                state = GameState.StartScreen;
            }
            else
            {
                state = GameState.Escape;
                Hero.instance.powerupBoost = 1;
                Hero.instance.powerupDrill = 2;
                Hero.instance.powerupGun = 2;
                Hero.instance.powerupRetro = 2;
                Hero.instance.powerupRadar = 1;
            }
            game = this;
            for (int i = -10; i < 7; i++)
                Console.WriteLine("" + i % 5);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            if (screenHeight <= 750)
                setScreenSize(ScreenSize.Small);
            else if (screenHeight <= 850)
                setScreenSize(ScreenSize.Medium);
            else if (screenHeight <= 950)
                setScreenSize(ScreenSize.Large);
            else if (screenHeight > 950)
                setScreenSize(ScreenSize.Huge);
            else
                setScreenSize(ScreenSize.Small);

            PIXEL = new Texture2D(graphics.GraphicsDevice, 1, 1);
            PIXEL.SetData(new[] { Color.White });
            viewport = GraphicsDevice.Viewport;
            History.setEffectRadiusMax();

            graphics.IsFullScreen = false;
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.Location = new System.Drawing.Point((int)(screenWidth - screenSize.X) / 2, 5);

            base.Initialize();
        }

        public static void setScreenSize(ScreenSize size)
        {
            currentScreenSizeMode = size;
            game.setScreenSize((int)size);
            Vignette.Load();
        }

        private void setScreenSize(int index)
        {
            levelOffsetFromHUD = LEVEL_OFFSETS_FROM_HUD[index];
            hudSize = HUD_SIZES[index];
            screenSize = new Vector2(SCREEN_SIZES[index], SCREEN_SIZES[index] + levelOffsetFromHUD);
            graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            graphics.PreferredBackBufferWidth = (int)screenSize.X;
            graphics.ApplyChanges();

            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, 
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            viewport = GraphicsDevice.Viewport;
            if (state == GameState.Arena)
            {
                levelManager.scrollCamera(new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + LevelManager.STARTING_TILE.X * Level.TILE_SIZE + Level.TILE_SIZE / 2,
                    LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + LevelManager.STARTING_TILE.Y * Level.TILE_SIZE + Level.TILE_SIZE / 2), 1);
            }
        }

        public static void toggleScreenSize()
        {
            if (state == GameState.PauseScreen || state == GameState.StartScreen)
                setScreenSize((ScreenSize)Enum.ToObject(typeof(ScreenSize), ((int)Game1.currentScreenSizeMode + 1) % 4));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //load sprites automatically so you don't need to put in a new line every time you add a new sprite
            TextureManager.LoadSprites(Content);

            FONT_DEBUG = Content.Load<SpriteFont>("Fonts\\debug");
            FONT_EXCLAMATION = Content.Load<SpriteFont>("Fonts\\pixel28"); /* http://www.dafont.com/visitor.font + http://xbox.create.msdn.com/en-US/education/catalog/utility/bitmap_font_maker */
            FONT_PIXEL_SMALL = Content.Load<SpriteFont>("Fonts\\pixel23");
            FONT_PIXEL_LARGE = Content.Load<SpriteFont>("Fonts\\pixel48");
            FONT_PIXEL_HUGE = Content.Load<SpriteFont>("Fonts\\pixel98");
            FONT_HUD_KEYS = Content.Load<SpriteFont>("Fonts\\keys36"); /* http://www.fontspace.com/flop-design/keymode-alphabet */


            //load sounds
            Aragonaise = Content.Load<Song>("Audio\\Waves\\Aragonaise");
            MediaPlayer.IsRepeating = true;

            //misc loads/inits
            Hero.Initialize(0);
            Names.Initialize();
            RiotGuardWall.Initialize(0);

            //create graphics ojects
            graphicsDevice = GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);
            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            //load effects
            Effects.LoadContent(Content);

            //load levels
            Level.Load(Content);
            FileInfo[] filePaths = new DirectoryInfo(Content.RootDirectory + "\\Levels").GetFiles("*.*");
            foreach(FileInfo file in filePaths)
                levelTemplates[file.Name.Split('.')[0]] = new Level(Content.Load<LevelContent>("Levels\\" + file.Name.Split('.')[0]), file.Name.Split('.')[0],  spriteBatch);
            introLevelTemplate = levelTemplates["intro"];
            levelTemplates.Remove("intro");

            Reset();
        }

        // Reset game from game over to start over again
        public static void Reset(int checkpointToResetTo = 0)
        {
            // Levels
            Level.Initialize();
            levelManager.addLevel(new Level(introLevelTemplate, LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y), LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y);
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        levelManager.addLevel(new Level(levelTemplates.ElementAt(rand.Next(levelTemplates.Keys.Count)).Value, LevelManager.STARTING_LEVEL.X + i, LevelManager.STARTING_LEVEL.Y + j), LevelManager.STARTING_LEVEL.X + i, LevelManager.STARTING_LEVEL.Y + j);
            if (state != GameState.Escape)
            {
                // Game1
                score = 0;
                availableSand = (checkpointToResetTo >= 3) ? 1 : 0;
                retroStatisActive = false;
                currentEffect = null;
                drawEffects = false;

                // HUD
                exclamationDurationSecs = 0;
                currentExclamationTime = 0;
                HUD.showExclamation = false;

                // LevelManager
                levelManager.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + Level.TILE_SIZE / 2, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE - levelOffsetFromHUD - Level.TILE_SIZE * 2f);
                Hero newHero = new Hero();
                newHero.powerupBoost = levelManager.hero.powerupBoost;
                newHero.powerupGun = levelManager.hero.powerupGun;
                newHero.powerupRetro = levelManager.hero.powerupRetro;
                newHero.powerupDrill = levelManager.hero.powerupDrill;
                newHero.powerupRadar = levelManager.hero.powerupRadar;
                levelManager.hero = newHero;
                levelManager.setCenterEntity(levelManager.hero);
                levelManager.initializeArena();

                // Other components
                Powerups.Initialize(checkpointToResetTo);
                Hero.Initialize(checkpointToResetTo);
                RiotGuardWall.Initialize(checkpointToResetTo);
                History.clearFrames();
            }
            else
            {
                availableSand = 30;
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            Controller.Update(gameTime);
            if (state == GameState.Arena || state == GameState.Escape || state == GameState.RetroPort)
            {
                if (songstart)
                {
                    MediaPlayer.Stop();
                    songstart = false;
                }

                currentEffect = null;
                drawEffects = false;
                switch (state)
                {
                    case GameState.Arena:
                        RiotGuardWall.UpdateArena(gameTime);
                        levelManager.UpdateArena(gameTime);
                        Powerups.Update(gameTime);
                        if (Hero.instance.powerupRetro == 1)
                            History.UpdateArena(gameTime);
                        break;
                    case GameState.Escape:
                        RiotGuardWall.UpdateEscape(gameTime);
                        levelManager.scrolling = true;
                        levelManager.UpdateEscape(gameTime);
                        if (Hero.instance.powerupRetro == 1)
                            History.UpdateEscape(gameTime);
                        break;
                    case GameState.RetroPort:
                        Powerups.UpdateDying(gameTime);
                        RiotGuardWall.UpdateRetro(gameTime);
                        levelManager.UpdateRetro(gameTime);
                        History.UpdateRetro(gameTime);
                        break;
                }

                frameTimeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameTimeCounter >= 1000d)
                {
                    framerate = frameCounter / (frameTimeCounter / 1000d);
                    frameTimeCounter = 0;
                    frameCounter = 0;
                }

                vignetteIntensity += seconds * VIGNETTE_PULSE_SPEED * vignetteMultiplier;
                if (vignetteIntensity >= VIGNETTE_MAX_INTENSITY)
                {
                    vignetteMultiplier *= -1;
                }
                else if (vignetteIntensity < 0)
                {
                    vignetteMultiplier = 0;
                    drawVignette = false;
                }

                effectRadius += EFFECT_RADIUS_SPEED * effectRadiusMultiplier * seconds;
                if (effectRadius < EFFECT_RADIUS_MIN || effectRadius > EFFECT_RADIUS_MAX)
                {
                    effectRadiusMultiplier *= -1;
                }

            }
            if (state == GameState.GameOverScreen)
            {
                if (!songstart)
                {
                    MediaPlayer.Play(Aragonaise);
                    songstart = true;
                }
            }
            HUD.Update(gameTime);
            base.Update(gameTime);
        }

        public static void pressStartButton()
        {
            switch (state)
            {
                case GameState.Arena:
                case GameState.Escape:
                case GameState.RetroPort:
                    lastState = state;
                    state = GameState.PauseScreen;
                    break;
                case GameState.StartScreen:
                    state = GameState.Arena;
                    break;
                case GameState.PauseScreen:
                    state = lastState;
                    break;
                case GameState.GameOverScreen:
                    Reset(checkpoint);
                    state = GameState.StartScreen;
                    break;
            }
        }

        public static void gameOver()
        {
            state = GameState.GameOverScreen;
        }

        public static void enterEscapeMode()
        {
            pulseVignette();
            Powerups.enabled = false;
            Game1.levelManager.targetZoom = LevelManager.ZOOM_ESCAPE;
            state = GameState.Escape;
        }

        public static void addScore(int score)
        {
            Game1.score += score;
            lastScore = score;
        }

        public static void addSand()
        {
            if (availableSand < MAX_SAND)
                availableSand++;
        }

        public static void removeSand()
        {
            availableSand--;
        }

        public static void pulseVignette()
        {
            pulseVignette(Color.Red);
        }

        public static void pulseVignette(Color color)
        {
            vignetteIntensity = 0;
            vignetteColor = color;
            vignetteMultiplier = 1;
            drawVignette = true;
        }

        public static void showExclamation(string message, Color color, float duration)
        {
            exclamationStrings = new string[] { message };
            exclamationColors = new Color[] { color };
            exclamationDurationSecs = duration;
            currentExclamationTime = 0;
        }

        public static void showExclamation(string[] strings, Color[] colors, float durationSecs)
        {
            if (strings.Length == 0 || colors.Length == 0 || strings.Length != colors.Length)
            {
                throw new ArgumentOutOfRangeException("The arguments messages and colors must both be of the same size and not be empty.");
            }
            exclamationStrings = strings;
            exclamationColors = colors;
            exclamationDurationSecs = durationSecs;
            currentExclamationTime = 0;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            screenText = "";
            switch (state)
            {
                case GameState.StartScreen:
                    screenText = "      << Start Screen >>\n" +
                                 "        Press ENTER to \n" +
                                 "             start";
                    break;
                case GameState.PauseScreen:
                    screenText = "      << Pause Screen >>\n" +
                                 "        Press ENTER to \n" +
                                 "           continue";
                    break;
                case GameState.GameOverScreen:
                    screenText = "       << Game Over >>\n" +
                                 "       Press ENTER to \n" +
                                 "       go to checkpoint";
                    break;
            }

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(shaderRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            
            // Draw on offscreen render area using spriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                null, levelManager.getViewMatrix());
            if (drawLevelDebugTextures)
                levelManager.DrawDebug(spriteBatch);
            levelManager.Draw(spriteBatch);
            RiotGuardWall.Draw(spriteBatch);
            if (state != GameState.Escape)
                Powerups.Draw(spriteBatch);
            if (DEBUG)
            {
                spriteBatch.DrawString(FONT_DEBUG, "POWERUP ICON TEST:", new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 15, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 30 * Level.TILE_SIZE + 5), Color.White);
                Powerups.DrawDebug(spriteBatch, new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 250, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 30 * Level.TILE_SIZE + 16));
                RiotGuardWall.DrawDebug(spriteBatch);
            }
            spriteBatch.End();

            // Switch back to drawing onto the back buffer
            GraphicsDevice.SetRenderTarget(null);

            // Post-processing effects
            if (drawEffects)
            {
                if (currentEffect == null)
                {
                    throw new Exception("Make sure the currentEffect field is set explicitly in the update of every frame that you want the effect in");
                }
            }
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                currentEffect, Matrix.Identity);
            spriteBatch.Draw(shaderRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.End();

            // Draw on HUD/UI area using spriteBatchHUD
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            HUD.Draw(spriteBatchHUD);

            if (Hero.instance.powerupRadar == 1)
                levelManager.DrawRadar(spriteBatchHUD, HUD.hudScale);

            spriteBatchHUD.DrawString(FONT_DEBUG, "FPS: " + framerate.ToString("00.0"), new Vector2(15, hudSize), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            if (DEBUG)
            {
                spriteBatchHUD.DrawString(FONT_DEBUG, "Hero: " + Hero.instance.tileX + "/" + Hero.instance.tileY + "\nCell: " + levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].cellName + "\nWall Spd: " + RiotGuardWall.wallSpeed + "\nEnemy limit: " + Powerups.ENEMY_LIMITS_TOTAL_SPAWNED[Powerups.currentEnemyDifficulty] + "\nEnemy count: " + Powerups.enemiesSpawnedThisPhase, new Vector2(350, hudSize + 30), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
            if (state == GameState.PauseScreen || state == GameState.StartScreen)
            {
                if (Controller.currentInputType == InputType.Keyboard)
                    spriteBatchHUD.DrawString(FONT_DEBUG, "WASD/Arrow keys to MOVE\n[Shift] to BOOST\n[Space] to SHOOT\n[Q] to RETRO\n\n[Enter] to PAUSE\n[L] to change screen size", new Vector2(15, hudSize + 30), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                else
                    spriteBatchHUD.DrawString(FONT_DEBUG, "DPAD/Sticks to MOVE\n(RB) to BOOST\n(A) to SHOOT\n(X) to RETRO\n\n(Start) to PAUSE\n[L] to change screen size", new Vector2(15, hudSize + 30), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                
                spriteBatchHUD.DrawString(FONT_DEBUG, "Ver. ALPHA 0.4.1", new Vector2(screenSize.X - 210, hudSize), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);

                if (state == GameState.PauseScreen)
                    spriteBatchHUD.DrawString(FONT_DEBUG, "[F12] to RESET GAME", new Vector2(15, hudSize + 190), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }

            if (drawVignette)
                Vignette.Draw(spriteBatchHUD, vignetteColor, vignetteIntensity);
            if (DEBUG)
            {
                levelManager.DrawDebugHUD(spriteBatchHUD);
            }
            spriteBatchHUD.End();
            
            base.Draw(gameTime);
        }

        private static class HUD
        {
            public static readonly Color HUD_COLOR = Color.Navy;
            public static readonly Color COOLDOWN_COLOR = new Color(60, 60, 125);

            public static readonly int NUM_POWERUP_CELLS = 5;
            public static readonly int CELL_SIZE = 20;
            public static readonly int SPACE_BETWEEN_CELLS = 4;

            public static readonly Color COLOR_SLOW = Color.Lime;
            public static readonly Color COLOR_FAST = Color.Red;
            public static readonly Color COLOR_CURRENT = Color.White;
            public static readonly int CIRCLE_SIZE = 10;
            public static readonly int CURRENT_CIRCLE_SIZE = 13;
            public static readonly int HUD_GUARD_SIZE = 20;
            public static readonly int SPACE_BETWEEN_CIRCLES = 4;

            public static readonly Color SCORE_BORDER_COLOR = new Color(25, 184, 236);
            public static readonly Color[] SCORE_TEXT_COLORS = new Color[10] {
                new Color(255, 255, 255),
                new Color(255, 50, 50),
                
                new Color(150, 60, 255),
                new Color(255, 165, 0),
                
                new Color(255, 255, 50),
                new Color(50, 255, 50),
                
                new Color(255, 105, 180),
                new Color(220, 130, 80),
                
                new Color(40, 140, 40),
                new Color(12, 238, 188),
            };
            public static readonly Color SCORE_TEXT_COLOR_ZERO = Color.Gray;
            public static readonly int SCORE_DIGITS = 8;
            public static readonly int SCORE_BORDER_WIDTHHEIGHT = 2;
            public static readonly float SCORE_CELL_RELATIVEWIDTH = 0.048f;
            public static readonly float SCORE_CELL_RELATIVEHEIGHT = 0.50f;
            public static readonly float SCORE_TEXT_BASE_SCALE = 0.3f;

            public static readonly float RELATIVESPACE_BETWEEN_SCORE_AND_SAND = 0.23f;

            //public static readonly int SAND_PER_ROW = 5;
            public static readonly int SAND_SIZE = 17;
            public static readonly int SPACE_BETWEEN_SAND = 1;

            public static readonly float WALL_SPEED_POSITION_RELATIVE_X = 0.74f;
            public static readonly float WALL_SPEED_POSITION_RELATIVE_Y = 0.70f;

            public static readonly float[] HUD_SCALES = { 1f, 1.30f, 1.60f, 1.95f };
            public static float hudScale = 1f;
            public static bool showExclamation;

            public static void Update(GameTime gameTime)
            {
                float seconds = gameTime.getSeconds();
                hudScale = HUD_SCALES[(int)currentScreenSizeMode];

                showExclamation = false;
                if (exclamationDurationSecs > 0)
                {
                    currentExclamationTime += seconds;
                    if (currentExclamationTime <= exclamationDurationSecs)
                        showExclamation = true;
                }
            }

            public static void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(PIXEL, new Rectangle(0, 0, (int)screenSize.X, hudSize), HUD_COLOR);

                // powerups
                float spaceBetweenCells = SPACE_BETWEEN_CELLS * hudScale;
                float cellSize = CELL_SIZE * hudScale;

                float xPos = 0;
                float yPos = SPACE_BETWEEN_CELLS * hudScale;
                for (int i = 0; i < NUM_POWERUP_CELLS; i++)
                {
                    xPos += spaceBetweenCells;
                    spriteBatch.Draw(PIXEL, new Rectangle((int)xPos, (int)yPos, (int)cellSize, (int)cellSize), Color.White);
                    Texture2D powerupIcon = null;
                    bool displayPowerupBind = false;
                    switch (i)
                    {
                        case 0:
                            if (Hero.instance.powerupBoost == 1)
                            {
                                powerupIcon = TextureManager.Get("boosticon1");
                                displayPowerupBind = true;
                            }
                            else if (Hero.instance.powerupBoost == 2)
                                powerupIcon = TextureManager.Get("boosticon2");
                            break;
                        case 1:
                            displayPowerupBind = true;
                            if (Hero.instance.powerupGun == 1)
                                powerupIcon = TextureManager.Get("forwardshoticon1");
                            else if (Hero.instance.powerupGun == 2)
                                powerupIcon = TextureManager.Get("sideshoticon1");
                            else if (Hero.instance.powerupGun == 3)
                                powerupIcon = TextureManager.Get("chargeshoticon1");
                            else
                                displayPowerupBind = false;
                            break;
                        case 2:
                            displayPowerupBind = true;
                            if (Hero.instance.powerupRetro == 1)
                                powerupIcon = TextureManager.Get("retroicon1");
                            else if (Hero.instance.powerupRetro == 2)
                                powerupIcon = TextureManager.Get("retroicon2");
                            else
                                displayPowerupBind = false;
                            break;
                        case 3:
                            if (Hero.instance.powerupDrill == 1)
                                powerupIcon = TextureManager.Get("drillicon1");
                            else if (Hero.instance.powerupDrill == 2)
                                powerupIcon = TextureManager.Get("drillicon2");
                            break;
                        case 4:
                            if (Hero.instance.powerupRadar == 1)
                                powerupIcon = TextureManager.Get("radaricon1");
                            break;
                    }
                    float powerupCharge = Hero.instance.getPowerupCharge(i);
                    if (powerupIcon != null)
                    {
                        float powerupIconScale = (float)CELL_SIZE / 64 * hudScale;
                        spriteBatch.Draw(powerupIcon, new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, powerupIconScale, SpriteEffects.None, 0);
                    }
                    if (powerupCharge < 1)
                        spriteBatch.Draw(PIXEL, new Rectangle((int)xPos, (int)yPos, (int)cellSize, (int)cellSize), COOLDOWN_COLOR.withAlpha(75));
                    float maskSize = cellSize + 1;
                    spriteBatch.Draw(PIXEL, new Rectangle((int)xPos, (int)(yPos), (int)cellSize, (int)(maskSize * (1 - powerupCharge))), COOLDOWN_COLOR.withAlpha(150));
                    if (displayPowerupBind)
                        if (Controller.currentInputType == InputType.Keyboard)
                        {
                            spriteBatch.DrawString(FONT_HUD_KEYS, Controller.getKeyIconForPowerup(i + 1), new Vector2(xPos, yPos + cellSize + 6 * hudScale), Color.White, 0, Vector2.Zero, 0.4f * hudScale, SpriteEffects.None, 0);
                        }
                        else
                        {
                            spriteBatch.DrawString(FONT_HUD_KEYS, Controller.getButtonIconForPowerup(i + 1), new Vector2(xPos, yPos + cellSize + 6 * hudScale), Color.White, 0, Vector2.Zero, 0.4f * hudScale, SpriteEffects.None, 0);
                        }
                    xPos += cellSize;
                }
                xPos += 2 * spaceBetweenCells;

                // score
                float xPosScore = xPos;
                float yPosScore = yPos;
                float scoreBorderWidthHeight = SCORE_BORDER_WIDTHHEIGHT * hudScale;
                for (int i = 0; i <= SCORE_DIGITS; i++)
                {
                    spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)(xPosScore + i * SCORE_CELL_RELATIVEWIDTH * screenSize.X), (int)(yPosScore), (int)scoreBorderWidthHeight, (int)(SCORE_CELL_RELATIVEHEIGHT * hudSize + scoreBorderWidthHeight)), SCORE_BORDER_COLOR);
                }
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)(xPosScore), (int)(yPosScore), (int)(SCORE_DIGITS * SCORE_CELL_RELATIVEWIDTH * screenSize.X + scoreBorderWidthHeight), (int)scoreBorderWidthHeight), SCORE_BORDER_COLOR);
                spriteBatch.Draw(Game1.PIXEL, new Rectangle((int)(xPosScore), (int)(yPosScore + SCORE_CELL_RELATIVEHEIGHT * hudSize), (int)(SCORE_DIGITS * SCORE_CELL_RELATIVEWIDTH * screenSize.X + scoreBorderWidthHeight), (int)scoreBorderWidthHeight), SCORE_BORDER_COLOR);
                xPosScore += (SCORE_CELL_RELATIVEWIDTH * screenSize.X) / 2;
                yPosScore += (SCORE_CELL_RELATIVEHEIGHT * hudSize) / 2;
                float scoreTextScale = SCORE_TEXT_BASE_SCALE * hudScale;
                for(int i = SCORE_DIGITS - 1; i >= 0; i--)
                {
                    int dividedScore = score / (int)Math.Pow(10, i); 
                    int digit = dividedScore % 10;   
                    Color dcolor = (dividedScore == 0) ? SCORE_TEXT_COLOR_ZERO : SCORE_TEXT_COLORS[digit];
                    string dstring = "" + digit;
                    Vector2 dimensions = FONT_PIXEL_HUGE.MeasureString(dstring);
                    spriteBatch.DrawString(FONT_PIXEL_HUGE, "" + digit, new Vector2(xPosScore, yPosScore), dcolor, 0, dimensions / 2, scoreTextScale, SpriteEffects.None, 0);
                    xPosScore += (SCORE_CELL_RELATIVEWIDTH * screenSize.X);
                }

                // sand
                float xPosSand = xPos;
                float yPosSand = (int)(yPos + (RELATIVESPACE_BETWEEN_SCORE_AND_SAND + SCORE_CELL_RELATIVEHEIGHT) * hudSize);
                float sandSize = SAND_SIZE * hudScale;
                xPosSand += SAND_SIZE / 2;
                float spaceBetweenSand = SPACE_BETWEEN_SAND * hudScale;
                float sandScale = (float)SAND_SIZE / 64 * hudScale;
                for (int i = 0; i < availableSand; i++)
                {
                    spriteBatch.Draw(TextureManager.Get("sandiconhud"), new Vector2(xPosSand, yPosSand), null, Color.White, 0, new Vector2(32, 32), sandScale, SpriteEffects.None, 0);
                    xPosSand += sandSize + spaceBetweenSand;
                }

                if (Hero.instance.powerupRadar == 1)
                {
                    // riot wall speed
                    xPos = screenSize.X * WALL_SPEED_POSITION_RELATIVE_X;
                    yPos = hudSize * WALL_SPEED_POSITION_RELATIVE_Y;
                    float circleSize = CIRCLE_SIZE * hudScale;
                    float spaceBetweenCircles = SPACE_BETWEEN_CIRCLES * hudScale;
                    float circleScale = (float)CIRCLE_SIZE / 64 * hudScale;
                    float currentCircleScale = (float)CURRENT_CIRCLE_SIZE / 64 * hudScale;
                    float guardScale = (float)HUD_GUARD_SIZE / 64 * hudScale;
                    float guardToCircleScaleRatio = guardScale / currentCircleScale;

                    spriteBatch.DrawString(FONT_PIXEL_SMALL, "Dist: " + (int)(Hero.instance.position.X - RiotGuardWall.wallPosition), new Vector2(xPos, 0), Color.Orange, 0f, Vector2.Zero, hudScale * 0.8f, SpriteEffects.None, 0f);

                    xPos -= (circleSize + spaceBetweenCircles) + ((circleSize + spaceBetweenCircles) * (guardToCircleScaleRatio - 1));
                    spriteBatch.Draw(TextureManager.Get("riotguard1"), new Vector2(xPos, yPos), null, COLOR_CURRENT, (float)Math.PI * 3 / 2, new Vector2(32, 32), guardScale, SpriteEffects.None, 0);
                    xPos += (circleSize + spaceBetweenCircles) + ((circleSize + spaceBetweenCircles) * (guardToCircleScaleRatio - 1));
                    for (int i = 0; i < RiotGuardWall.getWallSpeedCount(); i++)
                    {
                        if (RiotGuardWall.getCurrentWallSpeedIndex() == i)
                        {
                            spriteBatch.Draw(TextureManager.Get("largecircle"), new Vector2(xPos, yPos), null, COLOR_CURRENT, 0, new Vector2(32, 32), currentCircleScale, SpriteEffects.None, 0);
                        }
                        float perc = (float)i / RiotGuardWall.getWallSpeedCount();
                        spriteBatch.Draw(TextureManager.Get("largecircle"), new Vector2(xPos, yPos), null, Color.Lerp(COLOR_SLOW, COLOR_FAST, perc), 0, new Vector2(32, 32), circleScale, SpriteEffects.None, 0);
                        xPos += circleSize + spaceBetweenCircles;
                    }
                    xPos += ((circleSize + spaceBetweenCircles) * (guardToCircleScaleRatio - 1));
                    spriteBatch.Draw(TextureManager.Get("riotguardangry"), new Vector2(xPos, yPos), null, COLOR_CURRENT, (float)Math.PI * 3 / 2, new Vector2(32, 32), guardScale, SpriteEffects.None, 0);
                }

                // on-screen text
                if (screenText.Length > 0)
                {
                    spriteBatch.DrawString(FONT_PIXEL_LARGE, screenText, new Vector2((screenSize.X - 700) / 2, screenSize.Y / 2), Color.White);
                }
                else if (showExclamation &&  exclamationStrings != null && exclamationStrings.Length > 0)
                {
                    float exclamationScale = (hudScale + 1.25f) / 2;
                    float space = FONT_EXCLAMATION.MeasureString(" ").X;
                    string fullString = "";
                    foreach (string s in exclamationStrings)
                        fullString += s + " ";
                    fullString = fullString.Trim();
                    Vector2 fullDimensions = FONT_EXCLAMATION.MeasureString(fullString);
                    float initialPos = (screenSize.X - fullDimensions.X * exclamationScale) / 2;
                    float offset = 0;
                    for (int i = 0; i < exclamationStrings.Length; i++)
                    {
                        string s = exclamationStrings[i];
                        Color c = exclamationColors[i];
                        float stringWidth = FONT_EXCLAMATION.MeasureString(s).X;
                        spriteBatch.DrawString(FONT_EXCLAMATION, s, new Vector2(initialPos + offset, (screenSize.Y * 0.8f - fullDimensions.Y * exclamationScale) / 2), c, 0, Vector2.Zero, exclamationScale, SpriteEffects.None, 0);
                        offset += (stringWidth + space) * exclamationScale;
                    }
                }
            }
        }
    }
}
