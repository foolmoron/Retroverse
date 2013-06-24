using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Particles;
using System.IO;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Diagnostics;

namespace Retroverse
{
    public class RetroGame : Game
    {
        public const string VERSION_NAME = "ALPHA 0.2.0d";
#if DEBUG
        public const bool DEBUG = true;
        public static readonly string VERSION = "Ver. " + VERSION_NAME + " [DEBUG " + ProjectBuildDate.RetrieveLinkerTimestamp().ToShortDateString() + "]";
#else
        public const bool DEBUG = false;
        public static readonly string VERSION = "Ver. " + VERSION_NAME + " [RELEASE " + ProjectBuildDate.RetrieveLinkerTimestamp().ToShortDateString() + "]";
#endif
        public static readonly List<string> LEVELS_TO_TEST = new List<string>()
        {
            /* Leave this list empty for normal operation, otherwise
             * add the names of levels that you want to test 
             * here, separated by commas.  Ex:
             * "rachel1", "cornerTL", "horizBot"
             */
            "momin1", "momin2"
        };
        public static List<LevelFragment> testFragmentsFull = new List<LevelFragment>();
        public static List<LevelFragment> testFragmentsHalfHorizontal = new List<LevelFragment>();
        public static List<LevelFragment> testFragmentsHalfVertical = new List<LevelFragment>();
        public static List<LevelFragment> testFragmentsCorner = new List<LevelFragment>();

        public static readonly bool drawLevelDebugTextures = true && DEBUG;
        public static readonly bool INVINCIBILITY = false && DEBUG;
        public static RetroGame game;

        public static Random rand = new Random();
        public static GraphicsDeviceManager graphics;
        public static string EXECUTABLE_ROOT_DIRECTORY;

        public static ScreenSize currentScreenSizeMode = ScreenSize.Small;
        public static readonly int[] SCREEN_SIZES = new int[] { 600, 700, 800, 900 };
        public static readonly int[] LEVEL_OFFSETS_FROM_HUD = new int[] { 80, 90, 100, 110 };
        public static int levelOffsetFromHUD;
        public static Vector2 screenSize;
        public static bool screenSizeChanged = false;
        public static Texture2D PIXEL;
        public static SpriteFont FONT_DEBUG;
        public static SpriteFont FONT_PIXEL_SMALL;
        public static SpriteFont FONT_HUD_KEYS;
        public static SpriteFont FONT_HUD_XBOX;
        public static Viewport viewport;
        public static bool retroStatisActive = false;
        public static float timeScale = 1f;

        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch spriteBatchHUD;

        private static Stack<Screen> screenStack = new Stack<Screen>();
        public static GameState State { get; private set; }
        public static EscapeScreen EscapeScreen { get; private set; }
        private static Stack<LevelManagerScreen> levelManagerScreenStack = new Stack<LevelManagerScreen>();
        public static LevelManagerScreen TopLevelManagerScreen { get { return (levelManagerScreenStack.Count > 0) ? levelManagerScreenStack.Peek() : null; } }
        public static Screen TopScreen { get { return screenStack.Peek(); } }
        
        public static readonly Dictionary<string, Color> LEVEL_COLORS = new Dictionary<string, Color>();
        public static Dictionary<string, LevelFragment> levelFragmentsFull = new Dictionary<string, LevelFragment>();
        public static Dictionary<string, LevelFragment> levelFragmentsHalfHorizontal = new Dictionary<string, LevelFragment>();
        public static Dictionary<string, LevelFragment> levelFragmentsHalfVertical = new Dictionary<string, LevelFragment>();
        public static Dictionary<string, LevelFragment> levelFragmentsCorner = new Dictionary<string, LevelFragment>();
        public static LevelFragment IntroLevelFragment { get { return NUM_PLAYERS == 2 ? introCoopLevelFragment : introSingleLevelFragment; } }
        public static LevelFragment introSingleLevelFragment;
        public static LevelFragment introCoopLevelFragment;
        public static LevelFragment StoreLevelFragment;


        public const float STORE_CHARGE_TIME = 120f; // secs
        public static float storeChargeTime = 0f;
        public static float StoreCharge { get; set; }

        // screens
        public static GameState lastState;
        public static SpriteFont FONT_PIXEL_LARGE;
        public static SpriteFont FONT_PIXEL_HUGE;
        public static string screenText;

        public const int SCREEN_CHANGE_INTERVAL_FRAMES = 3;
        public static int framesSinceLastScreenChange = 0;

        // fps counter
        public double frameRate;
        public double frameTimeCounter;
        public int frameCounter;

        // score and collectibles management
        public static int Score { get; private set; }
        private static int lastScore = 0;
        public const int MAX_GEMS = int.MaxValue;
        public static int AvailableGems { get; private set; }
        public const int MAX_SAND = 10;
        public static int AvailableSand { get; private set; }
        public const int MAX_BOMBS = 10;
        public static int AvailableBombs { get; private set; }
        public static bool HasDrilled = false;

        public static int TotalSand { get { return getHeroes().Sum(h => h.CollectedSand); } }
        public static int TotalBombs { get { return getHeroes().Sum(h => h.CollectedBombs); } }
        public static int TotalGems { get { return getHeroes().Sum(h => h.CollectedGems); } }
        public static int TotalPrisoners { get { return getHeroes().Sum(h => h.FreedPrisoners.Count); } }

        public const int MAX_PLAYERS = 2;
        public static int NUM_PLAYERS = 1;

        public static bool IsFirstTimePlaying { get; private set; }

        public RetroGame()
        {
            IsFirstTimePlaying = false;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            graphics.SynchronizeWithVerticalRetrace = false;
            TargetElapsedTime = new TimeSpan(10000000L / 60L); // target fps
            game = this;
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
            if (screenHeight <= 850)
                SetScreenSize(ScreenSize.Small);
            else if (screenHeight <= 950)
                SetScreenSize(ScreenSize.Medium);
            else if (screenHeight <= 1000)
                SetScreenSize(ScreenSize.Large);
            else if (screenHeight > 1000)
                SetScreenSize(ScreenSize.Huge);

            PIXEL = new Texture2D(graphics.GraphicsDevice, 1, 1);
            PIXEL.SetData(new[] { Color.White });
            viewport = GraphicsDevice.Viewport;

            graphics.IsFullScreen = false;
            Form form = (Form)Control.FromHandle(Window.Handle);
            form.Location = new System.Drawing.Point((int)(screenWidth - screenSize.X) / 2, 5);            

            base.Initialize();
        }

        public static Hero[] getHeroes(LevelManagerScreen fromScreen = null)
        {
            if (fromScreen != null)
                return fromScreen.levelManager.heroes;
            else
                return TopLevelManagerScreen.levelManager.heroes;
        }

        public static Hero getMainLiveHero(LevelManagerScreen fromScreen = null)
        {
            Hero ret = null;
            if(fromScreen != null)
                ret = fromScreen.levelManager.heroes.FirstOrDefault(h => h.Alive);
            else
                ret = TopLevelManagerScreen.levelManager.heroes.FirstOrDefault(h => h.Alive);
            return ret; // null if all heroes are dead
        }

        public static Level[,] getLevels(LevelManagerScreen fromScreen = null)
        {
            if (fromScreen != null)
                return fromScreen.levelManager.levels;
            else
                return TopLevelManagerScreen.levelManager.levels;
        }

        public static void SetScreenSize(ScreenSize size)
        {
            currentScreenSizeMode = size;
            game.setScreenSize((int)size);
            HUD.UpdateScale();
            Vignette.Load();
        }

        public static void SetScreenSize(int index)
        {
            currentScreenSizeMode = (ScreenSize)Enum.ToObject(typeof(ScreenSize), index);
            game.setScreenSize(index);
            HUD.UpdateScale();
            Vignette.Load();
        }

        private void setScreenSize(int index)
        {
            levelOffsetFromHUD = LEVEL_OFFSETS_FROM_HUD[index];
            HUD.hudHeight = HUD.SIZES[index];
            screenSize = new Vector2(SCREEN_SIZES[index], SCREEN_SIZES[index] + levelOffsetFromHUD);
            graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            graphics.PreferredBackBufferWidth = (int)screenSize.X;
            graphics.ApplyChanges();

            screenSize = new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            foreach (Screen screen in screenStack)
                screen.OnScreenSizeChanged();
            viewport = GraphicsDevice.Viewport;
        }

        public static void toggleScreenSize()
        {
            SetScreenSize((ScreenSize)Enum.ToObject(typeof(ScreenSize), ((int)RetroGame.currentScreenSizeMode + 1) % 4));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            EXECUTABLE_ROOT_DIRECTORY = Path.GetDirectoryName(Application.ExecutablePath);

            //create graphics ojects
            graphicsDevice = GraphicsDevice;
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);

            //load sprites automatically so you don't need to put in a new line every time you add a new sprite
            TextureManager.LoadSprites(Content);

            //load screens
            EscapeScreen = new EscapeScreen();
            AddScreen(EscapeScreen, true);
            EscapeScreen.textureRandom = Content.Load<Texture2D>("Textures\\random");
            Powerups.LoadContent(Content);
            AddScreen(new StartScreen(getHeroes()[0].bindings), true);

            HUD.LoadContent(Content);

            FONT_DEBUG = Content.Load<SpriteFont>("Fonts\\debug");
            FONT_PIXEL_SMALL = Content.Load<SpriteFont>("Fonts\\pixel23"); //http://www.dafont.com/victors-pixel-font.font
            FONT_PIXEL_LARGE = Content.Load<SpriteFont>("Fonts\\pixel48");
            FONT_PIXEL_HUGE = Content.Load<SpriteFont>("Fonts\\pixel98");
            FONT_HUD_KEYS = Content.Load<SpriteFont>("Fonts\\keys36"); /* http://www.fontspace.com/flop-design/keymode-alphabet */
            FONT_HUD_XBOX = Content.Load<SpriteFont>("Fonts\\xbox32"); /* http://sinnix.net/downloads/?did=2 XNA Button Pack 3 - Jeff Jenkins (@Sinnix) */

            //load audio
            SoundManager.LoadContent(Content);
            SoundManager.SetMusicVolume(0);

            //misc loads/inits
            Highscores.Initialize();
            RiotGuardWall.Initialize();
            LevelManager.Load(GraphicsDevice);

            //load effects
            Effects.LoadContent(Content);

            //load levels
            Level.Load(Content);
            FileInfo[] filePaths = new DirectoryInfo(Content.RootDirectory + "\\Levels").GetFiles("*.*");
            foreach (FileInfo file in filePaths)
            {
                /*IF YOU ARE GETTING A CONTENTLOADEXCEPTION HERE, MAKE SURE TO SET THE CONTENT PROCESS OF YOUR LEVEL TO "Level Processor"*/
                LevelContent content = Content.Load<LevelContent>("Levels\\" + file.Name.Split('.')[0]);
                Dictionary<string, LevelFragment> levelFragmentsForCurrentTypeOfLevel = null;
                List<LevelFragment> testFragments = new List<LevelFragment>();
                switch (content.type)
                {
                    case LevelContent.Type.Full:
                        levelFragmentsForCurrentTypeOfLevel = levelFragmentsFull;
                        testFragments = testFragmentsFull;
                        break;
                    case LevelContent.Type.HalfHorizontal:
                        levelFragmentsForCurrentTypeOfLevel = levelFragmentsHalfHorizontal;
                        testFragments = testFragmentsHalfHorizontal;
                        break;
                    case LevelContent.Type.HalfVertical:
                        levelFragmentsForCurrentTypeOfLevel = levelFragmentsHalfVertical;
                        testFragments = testFragmentsHalfVertical;
                        break;
                    case LevelContent.Type.Corner:
                        levelFragmentsForCurrentTypeOfLevel = levelFragmentsCorner;
                        testFragments = testFragmentsCorner;
                        break;
                }
                LEVEL_COLORS[content.name] = content.color;
                if (content.color == Color.White)
                    Console.Out.WriteLine(content.name + " = white");
                if (!(content.name.Contains("intro") || content.name.Contains("store")))
                {
                    levelFragmentsForCurrentTypeOfLevel[content.name] = new LevelFragment(content, GraphicsDevice);
                }
                else if (content.name.Contains("introSingle"))
                    introSingleLevelFragment = new LevelFragment(content, GraphicsDevice);
                else if (content.name.Contains("introCoop"))
                    introCoopLevelFragment = new LevelFragment(content, GraphicsDevice);
                else if (content.name.Contains("store"))
                    StoreLevelFragment = new LevelFragment(content, GraphicsDevice);

                if (LEVELS_TO_TEST.Contains(content.name))
                    testFragments.Add(levelFragmentsForCurrentTypeOfLevel[content.name]);
            }

            //configuration file
            LoadConfig();
            if (IsFirstTimePlaying)
                ((StartScreen)TopScreen).enableDrawWASDInstructions = true;
            SaveConfig();

            Reset();
        }

        // Reset game from game over to start over again
        public static void Reset(SaveGame saveGame = null)
        {
            //LoadConfig();
            State = GameState.Arena;
            storeChargeTime = (DEBUG) ? STORE_CHARGE_TIME : 0;
            StoreCharge = 0;

            Prisoner.Initialize();
            History.ResetReversibles();
            if(saveGame != null)
                Level.Initialize(saveGame.cellOffset1, saveGame.cellOffset2);
            else
                Level.Initialize();
            retroStatisActive = false;
            EscapeScreen.levelManager.Camera = null; // make it use a whole new camera on initialize
            EscapeScreen.Reset(saveGame);
            if (saveGame != null)
            {
                saveGame.inventoryState.Restore();
                Score = saveGame.score;
                AvailableGems = saveGame.AvailableGems;
                AvailableSand = saveGame.AvailableSand;
                AvailableBombs = saveGame.AvailableBombs;
                HasDrilled = true;
                Saves.LastSaveFilename = saveGame.filename;
                Powerups.DummyPowerups[typeof(HealthPickup)].GemCost = saveGame.healthCost;
                Powerups.DummyPowerups[typeof(RevivePickup)].GemCost = saveGame.reviveCost;
            }
            else
            {
                Score = 0;
                AvailableGems = 0;
                AvailableSand = 0;
                AvailableBombs = 0;
                HasDrilled = false;
            }
            SoundManager.PlaySoundAsMusic("MainTheme");
            SoundManager.SetMusicVolumeSmooth(MenuScreen.BACKGROUND_MUSIC_VOLUME, 0.33f);
            Highscores.Initialize();
        }

        public static void Save()
        {
            Saves.DeleteLastSave();
            Saves.InitiateSave();
            SaveConfig();
        }

        public static SaveGame Load(string filename)
        {
            SaveGame loadedGame = null;
            if (filename != null)
            {
                loadedGame = Saves.InitiateLoad(filename);
                if (loadedGame != null)
                {
                    loadedGame.AvailableGems /= 2;
                    loadedGame.AvailableSand = Math.Min(loadedGame.AvailableSand, 1);
                    loadedGame.AvailableBombs = Math.Min(loadedGame.AvailableBombs, 1);
                }
            }
            else
                Saves.Reset();
            Reset(loadedGame);
            return loadedGame;
        }

        public static void SaveConfig()
        {
            Saves.InitiateConfigSave();
        }

        public static void LoadConfig()
        {
            ConfigAndBindings configAndBindings = Saves.InitiateConfigLoad();
            if (configAndBindings.config != null)
                configAndBindings.config.apply();
            else
            {
                IsFirstTimePlaying = true;
                ConfigSave.NewSave().apply();
            }
            if (configAndBindings.bindings != null)
                configAndBindings.bindings.apply();
            else
                BindingsSave.NewSave().apply();
        }

        public static bool HasSave()
        {
            return Saves.InitiateLoad() != null;
        }

        public static void ResetScores()
        {
            Highscores.Initialize(true);
            SaveConfig();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public static bool AddScreen(Screen screen, bool ignoreDelay = false)
        {
            if (ignoreDelay || RetroGame.framesSinceLastScreenChange >= RetroGame.SCREEN_CHANGE_INTERVAL_FRAMES)
            {
                framesSinceLastScreenChange = 0;
                screenStack.Push(screen);
                if (screen is LevelManagerScreen)
                    levelManagerScreenStack.Push((LevelManagerScreen)screen);
                if (!screen.Initialized)
                    screen.Initialize(graphicsDevice);
                return true;
            }
            return false;
        }

        public static bool PopScreen(bool ignoreDelay = false)
        {
            if (ignoreDelay || RetroGame.framesSinceLastScreenChange >= RetroGame.SCREEN_CHANGE_INTERVAL_FRAMES)
            {
                framesSinceLastScreenChange = 0;
                if (screenStack.Count == 1)
                    game.Exit();
                screenStack.Peek().Dispose();
                Screen screen = screenStack.Pop();
                if (screen is LevelManagerScreen)
                    levelManagerScreenStack.Pop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            framesSinceLastScreenChange++;

            Highscores.Update(gameTime);
            SoundManager.Update(gameTime);
            screenStack.Peek().Update(gameTime);
            Inventory.UpdateAcquired(gameTime);

            frameTimeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameTimeCounter >= 1000d)
            {
                frameRate = frameCounter / (frameTimeCounter / 1000d);
                frameTimeCounter = 0;
                frameCounter = 0;
            }
            base.Update(gameTime);
        }

        public static void PauseGame(Hero controllingPlayer)
        {
            if (AddScreen(new PauseScreen(controllingPlayer.bindings)))
            {
                SoundManager.PauseLoopingSounds();
                SoundManager.PlaySoundOnce("ButtonForward");
                TopLevelManagerScreen.OnPaused();
            }
        }

        public static void GameOver()
        {
            Highscores.Save();
            SaveConfig();
            AddScreen(new GameOverScreen(getHeroes()[0].bindings), true);
            SoundManager.StopMusic();
            SoundManager.StopLoopingSounds();
            SoundManager.PlaySoundOnce("GameOverTheme");
            SoundManager.SetMusicVolume(0);
            SoundManager.PlaySoundAsMusic("MainTheme");
            SoundManager.SetMusicVolumeSmooth(MenuScreen.BACKGROUND_MUSIC_VOLUME, 0.1f);
        }

        public static void EnterArenaMode()
        {
            TopLevelManagerScreen.levelManager.SetCameraMode(CameraMode.Arena);
            State = GameState.Arena;
        }

        public static void EnterEscapeMode()
        {
            TopLevelManagerScreen.levelManager.SetCameraMode(CameraMode.Escape);
            State = GameState.Escape;
            RiotGuardWall.StartMoving();
        }

        public static void AddScore(int score)
        {
            Score += score;
            lastScore = score;
        }

        public static void AddGem()
        {
            if (AvailableGems < MAX_GEMS)
                AvailableGems++;
        }

        public static void RemoveGems(int gems)
        {
            AvailableGems -= gems;
        }

        public static void AddSand()
        {
            if (AvailableSand < MAX_SAND)
                AvailableSand++;
        }

        public static void RemoveSand()
        {
            AvailableSand--;
        }

        public static void AddBomb()
        {
            if (AvailableBombs < MAX_BOMBS)
                AvailableBombs++;
        }

        public static void RemoveBomb()
        {
            AvailableBombs--;
        }

        public static void pulseVignette()
        {
            pulseVignette(Color.Red);
        }

        public static void pulseVignette(Color color)
        {
            EscapeScreen.pulseVignette(color);
        }

        public static LevelFragment getRandomLevelFragment(LevelContent.Type type)
        {
            switch (type)
            {
                case LevelContent.Type.Full:
                    if (testFragmentsFull.Count > 0)
                        return testFragmentsFull[rand.Next(testFragmentsFull.Count)];
                    return levelFragmentsFull.ElementAt(rand.Next(levelFragmentsFull.Keys.Count)).Value;
                case LevelContent.Type.HalfHorizontal:
                    if (testFragmentsHalfHorizontal.Count > 0)
                        return testFragmentsHalfHorizontal[rand.Next(testFragmentsHalfHorizontal.Count)];
                    return levelFragmentsHalfHorizontal.ElementAt(rand.Next(levelFragmentsHalfHorizontal.Keys.Count)).Value;
                case LevelContent.Type.HalfVertical:
                    if (testFragmentsHalfVertical.Count > 0)
                        return testFragmentsHalfVertical[rand.Next(testFragmentsHalfVertical.Count)];
                    return levelFragmentsHalfVertical.ElementAt(rand.Next(levelFragmentsHalfVertical.Keys.Count)).Value;
                case LevelContent.Type.Corner:
                    if (testFragmentsCorner.Count > 0)
                        return testFragmentsCorner[rand.Next(testFragmentsCorner.Count)];
                    return levelFragmentsCorner.ElementAt(rand.Next(levelFragmentsCorner.Keys.Count)).Value;
                default:
                    throw new ArgumentException("Incompatible level fragment type: " + type.ToString(), "type");
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        float oldSeconds = 0;
        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;
            float newSeconds = gameTime.getSeconds();
            if (oldSeconds != newSeconds)
            {
                // Console.WriteLine("Framerate changed at time " + gameTime.TotalGameTime.TotalSeconds.ToString("#.000") + " to " + newSeconds.ToString("0.00000"));
            }
            oldSeconds = newSeconds;
            
            int deepestScreenToDraw = 0;
            Screen currentScreen = screenStack.ElementAt(deepestScreenToDraw);
            while (currentScreen.DrawPreviousScreen && deepestScreenToDraw < (screenStack.Count - 1))
            {
                deepestScreenToDraw++;
                currentScreen = screenStack.ElementAt(deepestScreenToDraw);
            }

            //draw extra stuff
            for (int i = deepestScreenToDraw; i >= 0; i--)
            {
                screenStack.ElementAt(i).PreDraw(gameTime);
            }
            //draw final scene
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            for (int i = deepestScreenToDraw; i >= 0; i--)
            {
                screenStack.ElementAt(i).Draw(gameTime);
            }

            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatchHUD.DrawString(FONT_DEBUG, "FPS: " + frameRate.ToString("00.0"), new Vector2(screenSize.X - 115, screenSize.Y - 25), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatchHUD.End();

            base.Draw(gameTime);
        }

        public static IMemento GenerateMementoFromCurrentFrame()
        {
            return new RetroGameMemento();
        }

        private class RetroGameMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            GameState state;
            int availableBombs;

            public RetroGameMemento()
            {
                state = RetroGame.State;
                availableBombs = RetroGame.AvailableBombs;
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                if (isNewFrame)
                {
                    RetroGame.State = state;
                    RetroGame.AvailableBombs = availableBombs;
                }
            }
        }
    }
}
