using System;
using System.Collections.Generic;
using System.Linq;
using LevelPipeline;
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
        private static Game1 game;
        public static readonly bool TEST_INTRO_ARENA = true;
        public static readonly bool INVINCIBILITY = false;

        public static readonly Dictionary<string, Color> LEVEL_COLORS = new Dictionary<string, Color>()
        {
            {"intro", Color.CornflowerBlue},
            {"3", Color.Orange},
            {"5", Color.LawnGreen},
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
        public static Viewport viewport;
        public static GameState state;
        public static bool retroStatisActive = false;
        public static float timeScale = 1f;
        public static GraphicsDevice graphicsDevice;
        public static Dictionary<string, Level> levelTemplates = new Dictionary<string, Level>();
        public static Level introLevel;

        // screens
        public static GameState lastState;
        public static SpriteFont FONT_PIXEL_LARGE;
        public static string screenText;

        // fps counter
        public double framerate;
        public double frameTimeCounter;
        public int frameCounter;

        public static readonly float VIGNETTE_MAX_INTENSITY = 1.0f;
        public static readonly float VIGNETTE_MIN_INTENSITY = 0.6f;
        public float vignetteIntensity = VIGNETTE_MIN_INTENSITY;
        public float vignetteMultiplier = 1f;
        public static readonly float VIGNETTE_PULSE_SPEED = 0.7f;

        public static readonly float EFFECT_RADIUS_MIN = 20f;
        public static readonly float EFFECT_RADIUS_MAX = 300f;
        public static readonly float EFFECT_RADIUS_SPEED = 250f;
        public static float effectRadiusMultiplier = 1;
        public static float effectRadius = EFFECT_RADIUS_MIN;

        // on-screen text excalamation
        public static SpriteFont FONT_EXCLAMATION;
        public static string exclamationText;
        public static Color exclamationColor;

        // score and sand management
        private static int score = 0;
        private static int lastScore = 0;
        public static int availableSand { get; private set; }

        public static LevelManager levelManager = new LevelManager();
        Song Aragonaise;
        bool songstart = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            TargetElapsedTime = new TimeSpan(10000000L / 60L); // target fps
            if (TEST_INTRO_ARENA)
            {
                state = GameState.StartScreen;
            }
            else
            {
                state = GameState.Escape;
                Hero.instance.powerUp1 = 1;
                Hero.instance.powerUp2 = 2;
                Hero.instance.powerUp3 = 2;
                Hero.instance.powerUp4 = 2;
                Hero.instance.powerUp5 = 1;
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
            graphics.IsFullScreen = false;
            setScreenSize(ScreenSize.Small);

            PIXEL = new Texture2D(graphics.GraphicsDevice, 1, 1);
            PIXEL.SetData(new[] { Color.White });
            viewport = GraphicsDevice.Viewport;
            History.setEffectRadiusMax();

            base.Initialize();
            //levelManager.initializeEnemies();
            //levelManager.addEnemy(2, 2, 0,);
        }

        public static void setScreenSize(ScreenSize size)
        {
            currentScreenSizeMode = size;
            game.setScreenSize((int)size);
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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //load sprites
            TextureManager.SetContent(Content);
            TextureManager.Add("hero");
            TextureManager.Add("collectable1");
            TextureManager.Add("GunPower");
            TextureManager.Add("BootPower");
            TextureManager.Add("circle");
            TextureManager.Add("prisoner1");
            TextureManager.Add("prisonerhat1");
            TextureManager.Add("riotguard1");
            TextureManager.Add("bullet1");
            TextureManager.Add("bullet2");
            TextureManager.Add("chargebullet1");
            TextureManager.Add("chargebullet2");
            TextureManager.Add("chargebullet3");
            TextureManager.Add("enemy1");
            TextureManager.Add("enemy2");
            TextureManager.Add("enemy3");
            TextureManager.Add("enemy4");
            TextureManager.Add("sandicon");
            TextureManager.Add("gunicon");
            TextureManager.Add("sideshoticon1");
            TextureManager.Add("forwardshoticon1");
            TextureManager.Add("chargeshoticon1");
            TextureManager.Add("boosticon");
            TextureManager.Add("radaricon");
            TextureManager.Add("retroicon");
            TextureManager.Add("retroicon", 2);
            TextureManager.Add("drillicon");
            TextureManager.Add("drillicon", 2);
            TextureManager.Add("fairyglow", 4);

            FONT_DEBUG = Content.Load<SpriteFont>("Fonts\\debug");
            FONT_EXCLAMATION = Content.Load<SpriteFont>("Fonts\\pixel28"); /* http://www.dafont.com/visitor.font + http://xbox.create.msdn.com/en-US/education/catalog/utility/bitmap_font_maker */
            FONT_PIXEL_SMALL = Content.Load<SpriteFont>("Fonts\\pixel23");
            FONT_PIXEL_LARGE = Content.Load<SpriteFont>("Fonts\\pixel48");

            //load sounds
            Aragonaise = Content.Load<Song>("Audio\\Waves\\Aragonaise");
            MediaPlayer.IsRepeating = true;

            //misc loads/inits
            Hero.Initialize();
            Names.Initialize();
            RiotGuardWall.Initialize();

            //create graphics ojects
            graphicsDevice = GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);
            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            //load effects
            Effects.LoadContent(Content);
            Vignette.Load(spriteBatch);

            //load levels
            Level.Load(Content);
            FileInfo[] filePaths = new DirectoryInfo(Content.RootDirectory + "\\Levels").GetFiles("*.*");
            foreach(FileInfo file in filePaths)
                levelTemplates[file.Name.Split('.')[0]] = new Level(Content.Load<LevelContent>("Levels\\" + file.Name.Split('.')[0]), file.Name.Split('.')[0],  spriteBatch);
            introLevel = levelTemplates["intro"];
            levelTemplates.Remove("intro");

            Reset();
        }

        // Reset game from game over to start over again
        public static void Reset()
        {
            Level.Initialize();
            levelManager.addLevel(new Level(introLevel, LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y), LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y);
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    if (i != 0 || j != 0)
                        levelManager.addLevel(new Level(levelTemplates.ElementAt(rand.Next(levelTemplates.Keys.Count)).Value, LevelManager.STARTING_LEVEL.X + i, LevelManager.STARTING_LEVEL.Y + j), LevelManager.STARTING_LEVEL.X + i, LevelManager.STARTING_LEVEL.Y + j);
            if (state != GameState.Escape)
            {
                levelManager.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + Level.TILE_SIZE / 2, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE - levelOffsetFromHUD - Level.TILE_SIZE * 2f);
                levelManager.hero = new Hero();
                levelManager.initializeArena();
                availableSand = 0;
                Powerups.Initialize();
                Hero.Initialize();
                RiotGuardWall.Initialize();
            }
            else
            {
                availableSand = 2;
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
                currentEffect = null;
                drawEffects = false;
                exclamationText = "";
                switch (state)
                {
                    case GameState.Arena:
                        levelManager.UpdateArena(gameTime);
                        Powerups.Update(gameTime);
                        if (Hero.instance.powerUp4 == 1)
                            History.UpdateEscape(gameTime);
                        break;
                    case GameState.Escape:
                        RiotGuardWall.UpdateEscape(gameTime);
                        levelManager.scrolling = true;
                        levelManager.UpdateEscape(gameTime);
                        if (Hero.instance.powerUp4 == 1)
                            History.UpdateEscape(gameTime);
                        break;
                    case GameState.RetroPort:
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
                if (vignetteIntensity > VIGNETTE_MAX_INTENSITY || vignetteIntensity < VIGNETTE_MIN_INTENSITY)
                    vignetteMultiplier *= -1;

                effectRadius += EFFECT_RADIUS_SPEED * effectRadiusMultiplier * seconds;
                if (effectRadius < EFFECT_RADIUS_MIN || effectRadius > EFFECT_RADIUS_MAX)
                {
                    effectRadiusMultiplier *= -1;
                }

                //if (!songstart)
                //{
                //    MediaPlayer.Play(Aragonaise);
                //    songstart = true;
                //}

                HUD.Update(gameTime);
            }
            base.Update(gameTime);
        }

        public static void startButton()
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
                    Reset();
                    state = GameState.StartScreen;
                    break;
            }
        }

        public static void gameOver()
        {
            state = GameState.GameOverScreen;
        }

        public static void addScore(int score)
        {
            Game1.score += score;
            lastScore = score;
        }

        public static void addSand()
        {
            availableSand++;
        }

        public static void removeSand()
        {
            availableSand--;
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
                                 "           ... start";
                    break;
                case GameState.PauseScreen:
                    screenText = "      << Pause Screen >>\n" +
                                 "        Press ENTER to \n" +
                                 "           continue";
                    break;
                case GameState.GameOverScreen:
                    screenText = "       << Game Over >>\n" +
                                 "       Press ENTER to \n" +
                                 "      start a new game";
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
            if (state == GameState.Arena)
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

            if (Hero.instance.powerUp5 > 0)
                levelManager.DrawRadar(spriteBatchHUD);

            if (DEBUG)
            {
                spriteBatchHUD.DrawString(FONT_DEBUG, "Shift to BOOST\nSpace to SHOOT\nX to RETRO\nHero: " + Hero.instance.position + "\nCell: " + levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].cellName + "\nWall Spd: " + RiotGuardWall.wallSpeed, new Vector2(15, 70), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
            spriteBatchHUD.DrawString(FONT_DEBUG, "FPS: " + framerate.ToString("00.0"), new Vector2(screenSize.X - 120, 0.8f * hudSize), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);


            if (drawVignette)
                Vignette.Draw(spriteBatchHUD, Color.Red, vignetteIntensity);
            if (DEBUG)
            {
                levelManager.DrawDebugHUD(spriteBatchHUD);
            }
            spriteBatchHUD.End();
            
            base.Draw(gameTime);
        }

        private static class HUD
        {
            public static void Update(GameTime gameTime)
            {
                float seconds = gameTime.getSeconds();

            }

            public static void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(PIXEL, new Rectangle(0, 0, (int)screenSize.X, hudSize), Color.Navy);
                spriteBatch.DrawString(FONT_DEBUG, "Powerups: ", new Vector2(0, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);

                spriteBatch.Draw(TextureManager.Get("GunPower"), new Vector2(120, 2), Color.White);
                spriteBatch.Draw(TextureManager.Get("BootPower"), new Vector2(146, 2), Color.White);

                if (screenText.Length > 0)
                {
                    spriteBatch.DrawString(FONT_PIXEL_LARGE, screenText, new Vector2((screenSize.X - 700) / 2, screenSize.Y / 2), Color.White);
                }
                else if (exclamationText.Length > 0)
                {
                    int width = exclamationText.Length * 17;
                    spriteBatch.DrawString(FONT_EXCLAMATION, exclamationText, new Vector2((screenSize.X - width) / 2, screenSize.Y / 2), exclamationColor);
                }

                //spriteBatchHUD.Draw(t, new Rectangle(120, 2, 20, 20), Color.White);
                //spriteBatchHUD.Draw(t, new Rectangle(146, 2, 20, 20), Color.White);
                spriteBatch.Draw(PIXEL, new Rectangle(172, 2, 20, 20), Color.White);
                spriteBatch.Draw(PIXEL, new Rectangle(198, 2, 20, 20), Color.White);
                spriteBatch.Draw(PIXEL, new Rectangle(224, 2, 20, 20), Color.White);
                spriteBatch.Draw(PIXEL, new Rectangle(250, 2, 20, 20), Color.White);
                spriteBatch.DrawString(FONT_DEBUG, "Score: " + score + "\nSand: " + availableSand, new Vector2(screenSize.X / 2, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                spriteBatch.DrawString(FONT_DEBUG, "Dist: " + (int)(Hero.instance.position.X - RiotGuardWall.wallPosition), new Vector2(screenSize.X - 130, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
        }
    }
}
