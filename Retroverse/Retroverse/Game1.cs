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
    public enum GameState { Arena, Escape, RetroPort };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static readonly bool DEBUG = true;

        public static Random rand = new Random();
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public SpriteBatch spriteBatchHUD;
        public RenderTarget2D shaderRenderTarget;

        //DRAW OPTIONS
        public static bool drawLevelTextures =  true; // draw colored squares that back the levels
        public static bool drawVignette = false; // draw pulsing colored vignette
        public static bool drawEffects = false; // draw pulsing black and white effect

        public static readonly float HUD_PERCENTAGE = 0.1f;
        public static readonly float SCREEN_SIZE_PIXELS = 600;
        public static int hudSize = (int)(SCREEN_SIZE_PIXELS * HUD_PERCENTAGE);
        public static readonly int SCREEN_SIZE_MIN_HEIGHT = 640;
        public static readonly int SCREEN_SIZE_MAX_HEIGHT = 940;
        public static Vector2 screenSize = new Vector2(SCREEN_SIZE_PIXELS, SCREEN_SIZE_PIXELS + hudSize);
        public static bool screenSizeChanged = false;
        public static readonly float ASPECTRATIO = SCREEN_SIZE_PIXELS / (SCREEN_SIZE_PIXELS + hudSize);
        public static Texture2D PIXEL;
        public static SpriteFont FONT_DEBUG;
        public static Viewport viewport;
        public static GraphicsDevice graphicsDevice;
        public static Dictionary<string, Level> levelTemplates = new Dictionary<string, Level>();

        // history stuff
        public static readonly float RETROPORT_SECS = 3f;

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

        public static LevelManager levelManager = new LevelManager();
        Song Aragonaise;
        bool songstart = false;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(resizeWindow);
            TargetElapsedTime = new TimeSpan(10000000L / 60L); // target fps
        }

        void resizeWindow(object sender, EventArgs e)
        {
            Window.ClientSizeChanged -= new EventHandler<EventArgs>(resizeWindow);

            if (Window.ClientBounds.Width != screenSize.X)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / ASPECTRATIO);
            }
            else if (Window.ClientBounds.Height != screenSize.Y)
            {
                graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * ASPECTRATIO);
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }
            //limit window size
            graphics.PreferredBackBufferHeight = (int)MathHelper.Clamp(graphics.PreferredBackBufferHeight, SCREEN_SIZE_MIN_HEIGHT, SCREEN_SIZE_MAX_HEIGHT);
            graphics.PreferredBackBufferWidth = (int)(graphics.PreferredBackBufferHeight * ASPECTRATIO);

            graphics.ApplyChanges();
            screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
            hudSize = Window.ClientBounds.Height - Window.ClientBounds.Width;
            viewport = GraphicsDevice.Viewport;

            //new shaderRenderTarget
            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            Window.ClientSizeChanged += new EventHandler<EventArgs>(resizeWindow);
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
            graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            graphics.PreferredBackBufferWidth = (int)screenSize.X;
            graphics.ApplyChanges();

            PIXEL = new Texture2D(graphics.GraphicsDevice, 1, 1);
            PIXEL.SetData(new[] { Color.White });
            viewport = GraphicsDevice.Viewport;

            base.Initialize();
            //levelManager.initializeEnemies();
            //levelManager.addEnemy(2, 2, 0,);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Aragonaise = Content.Load<Song>("Audio\\Waves\\Aragonaise");
            MediaPlayer.IsRepeating = true;

            graphicsDevice = GraphicsDevice;
            TextureManager.SetContent(Content);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);

            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            //load effects
            Effects.LoadContent(Content);
            Vignette.Load(spriteBatch);

            //load levels
            Level.Initialize(Content);
            try
            {
                for (int i = 1; ; i++)
                    levelTemplates["" + i] = new Level(Content.Load<LevelContent>("Levels\\" + i), spriteBatch);
            }
            catch (ContentLoadException e)
            {
                Console.WriteLine(e);
            }
            levelManager.addLevel(levelTemplates["1"], 0, 0);
            levelManager.addLevel(levelTemplates["2"], 0, 1);
            levelManager.addLevel(levelTemplates["3"], 1, 0);
            levelManager.addLevel(levelTemplates["4"], 1, 1);

            //load sprites
            TextureManager.Add("hero");
            TextureManager.Add("bullet");
            TextureManager.Add("enemy_test");
            TextureManager.Add("collectable");
            TextureManager.Add("GunPower");
            TextureManager.Add("BootPower");
            FONT_DEBUG = Content.Load<SpriteFont>("Fonts\\debug");
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
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Controller.Update(gameTime);
            levelManager.Update(gameTime);
            History.Update(gameTime);

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
                outer = !outer;
                effectRadiusMultiplier *= -1;
            }

            base.Update(gameTime);

            //if (!songstart)
            //{
            //    MediaPlayer.Play(Aragonaise);
            //    songstart = true;
            //}
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        bool outer;
        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(shaderRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            
            // Draw on offscreen render area using spriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                null, levelManager.getViewMatrix());
            if (drawLevelTextures)
                levelManager.DrawDebug(spriteBatch);
            levelManager.Draw(spriteBatch);
            spriteBatch.End();

            // Switch back to drawing onto the back buffer
            GraphicsDevice.SetRenderTarget(null);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            //    DepthStencilState.None, RasterizerState.CullCounterClockwise,
            //    null, Matrix.Identity);
            //spriteBatch.Draw(shaderRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            //spriteBatch.End();
            // Post-processing effects
            Effect effect = null;
            if (drawEffects)
            {
                if (outer)
                    effect = Effects.OuterGrayscale;
                else
                    effect = Effects.Grayscale;
                effect.Parameters["width"].SetValue(screenSize.X);
                effect.Parameters["height"].SetValue(screenSize.Y);
                effect.Parameters["radius"].SetValue(effectRadius);
                effect.Parameters["intensity"].SetValue(5f);
            }
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                effect, Matrix.Identity);
            spriteBatch.Draw(shaderRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.End();

            // Draw on HUD/UI area using spriteBatchHUD
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            spriteBatchHUD.DrawString(FONT_DEBUG, "WASD to move\nQ and E to zoom/unzoom\nShift to toggle scrolling\nSpace to shoot\nX to rewind time\nAlt for rave party\nNumber of Collectables gotten:\n" + levelManager.collectablesToRemove.Count + "\nHero pos:" + Hero.instance.position, new Vector2(15, 30), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatchHUD.DrawString(FONT_DEBUG, "FPS: " + framerate.ToString("00.0"), new Vector2(screenSize.X - 120, 30), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatchHUD.Draw(PIXEL, new Rectangle(0, 0, (int)screenSize.X, 25), Color.Navy);


            spriteBatchHUD.DrawString(FONT_DEBUG, "Powerups: ", new Vector2(0, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);

            spriteBatchHUD.Draw(TextureManager.Get("GunPower"), new Vector2(120, 2), Color.White);
            spriteBatchHUD.Draw(TextureManager.Get("BootPower"), new Vector2(146, 2), Color.White);

            //spriteBatchHUD.Draw(t, new Rectangle(120, 2, 20, 20), Color.White);
            //spriteBatchHUD.Draw(t, new Rectangle(146, 2, 20, 20), Color.White);
            spriteBatchHUD.Draw(PIXEL, new Rectangle(172, 2, 20, 20), Color.White);
            spriteBatchHUD.Draw(PIXEL, new Rectangle(198, 2, 20, 20), Color.White);
            spriteBatchHUD.Draw(PIXEL, new Rectangle(224, 2, 20, 20), Color.White);
            spriteBatchHUD.Draw(PIXEL, new Rectangle(250, 2, 20, 20), Color.White);
            spriteBatchHUD.DrawString(FONT_DEBUG, "Score: ", new Vector2(screenSize.X / 2, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatchHUD.DrawString(FONT_DEBUG, "Time: " + (5 - gameTime.TotalGameTime.Minutes) + ":" + (60 - gameTime.TotalGameTime.Seconds),
                new Vector2(screenSize.X - 130, 0), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);

            levelManager.DrawRadar(spriteBatchHUD);
            if (drawVignette)
                //Vignette.Draw(spriteBatchHUD, Color.Red, vignetteIntensity);
            if (DEBUG)
            {
                levelManager.DrawDebugHUD(spriteBatch);
            }
            spriteBatchHUD.End();
            
            base.Draw(gameTime);
        }
    }
}
