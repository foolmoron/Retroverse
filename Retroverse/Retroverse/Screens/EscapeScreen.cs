using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class EscapeScreen : LevelManagerScreen
    {
        public const float BACKGROUND_MUSIC_VOLUME = 0.5f;

        public SpriteBatch spriteBatch;
        public SpriteBatch spriteBatchHUD;
        public RenderTarget2D shaderRenderTarget;

        public bool drawVignette = false;
        public static readonly float VIGNETTE_MAX_INTENSITY = 1.2f;
        public static readonly float VIGNETTE_MIN_INTENSITY = 0.0f;
        public float vignetteIntensity = VIGNETTE_MIN_INTENSITY;
        public float vignetteMultiplier = 1f;
        public static readonly float VIGNETTE_PULSE_SPEED = 1.2f;
        public Color vignetteColor = Color.Red;

        public Texture2D textureRandom;
        public RenderTarget2D testRenderTarget;
        public RenderTarget2D testRenderTarget2;
        public static readonly float TEST_EFFECT_RADIUS_MIN = 0f;
        public static readonly float TEST_EFFECT_RADIUS_MAX = 1f;
        public static readonly float TEST_EFFECT_RADIUS_SPEED = 0.25f;
        public float testEffectRadiusMultiplier = 1;
        public float testEffectRadius = TEST_EFFECT_RADIUS_MIN;

        public EscapeScreen()
        {
            DrawPreviousScreen = false;
            levelManager = new LevelManager();
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);
            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            testRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            testRenderTarget2 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public override void OnScreenSizeChanged()
        {
            shaderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            testRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            testRenderTarget2 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public override void OnPaused() { }

        public void Reset(SaveGame saveGame)
        {
            // HUD
            HUD.Initialize(GraphicsDevice);

            // Other components
            RiotGuardWall.Initialize(saveGame);

            // LevelManager last
            Point startingLevel = LevelManager.STARTING_LEVEL;
            if (saveGame != null)
            {
                startingLevel = new Point(saveGame.levelX, saveGame.levelY);
                Hero[] newHeroes = new Hero[RetroGame.NUM_PLAYERS];
                for(int i = 0; i < newHeroes.Length; i++)
                {
                    newHeroes[i] = new Hero(saveGame.heroStates[i]);
                }
                levelManager.heroes = newHeroes;
                levelManager.Initialize(RetroGame.NUM_PLAYERS, false, RetroGame.StoreLevelFragment, startingLevel);
            }
            else
            {
                levelManager.Initialize(RetroGame.NUM_PLAYERS, true, RetroGame.IntroLevelFragment, startingLevel);
            }
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    if (RetroGame.DEBUG)
                    {
                        int sum = (i + 1) + (j + 1) * 3;
                        levelManager.createRandomLevelAt(startingLevel.X + i, startingLevel.Y + j, (sum < 8) ? sum : 0);
                    }
                    else
                    {
                        levelManager.createRandomLevelAt(startingLevel.X + i, startingLevel.Y + j);
                    }
                }
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (i >= 0)
                        levelManager.levels[startingLevel.X + i, startingLevel.Y + j].updateLeftBorderColors();
                    if (j >= 0)
                        levelManager.levels[startingLevel.X + i, startingLevel.Y + j].updateTopBorderColors();
                    if (i >= 0 && j >= 0)
                        levelManager.levels[startingLevel.X + i, startingLevel.Y + j].updateCornerBorderColors();
                }
            levelManager.createAndRemoveLevels();
            
            currentEffect = null;
            drawEffects = false;
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            // LevelManager, Hero, and other components handle the inputs for this screen
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            if(SoundManager.TargetVolume != BACKGROUND_MUSIC_VOLUME)
                SoundManager.SetMusicVolumeSmooth(BACKGROUND_MUSIC_VOLUME);

            float prevStoreCharge = RetroGame.StoreCharge;
            if (!RiotGuardWall.IsWaiting)
                RetroGame.storeChargeTime += gameTime.getSeconds(1f);
            RetroGame.StoreCharge = RetroGame.storeChargeTime / RetroGame.STORE_CHARGE_TIME;
            if (prevStoreCharge < 1 && RetroGame.StoreCharge >= 1)
            {
                SoundManager.PlaySoundOnce("StoreChargedJingle");
            }


            currentEffect = null;
            drawEffects = false;
            switch (RetroGame.State)
            {
                case GameState.Arena:
                    levelManager.UpdateArena(gameTime);
                    break;
                case GameState.Escape:
                    levelManager.UpdateEscape(gameTime);
                    break;
            }
            RiotGuardWall.Update(gameTime);
            History.UpdateForward(gameTime);
            HUD.Update(gameTime);

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

            testEffectRadius += TEST_EFFECT_RADIUS_SPEED * testEffectRadiusMultiplier * seconds;
            if (testEffectRadius < TEST_EFFECT_RADIUS_MIN || testEffectRadius > TEST_EFFECT_RADIUS_MAX)
            {
                testEffectRadiusMultiplier *= -1;
            }
        }

        public void setZoom(float zoom)
        {
            levelManager.Camera.targetZoom = zoom;
        }

        public void pulseVignette(Color color)
        {
            vignetteIntensity = 0;
            vignetteColor = color;
            vignetteMultiplier = 1;
            drawVignette = true;
        }

        public override void PreDraw(GameTime gameTime)
        {
            LevelManager.PreDraw();

            GraphicsDevice.SetRenderTarget(shaderRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            // Draw on offscreen render area using spriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                null, levelManager.getViewMatrix());
            if (RetroGame.drawLevelDebugTextures)
                levelManager.DrawDebug(spriteBatch);
            levelManager.Draw(spriteBatch);
            RiotGuardWall.Draw(spriteBatch);
            if (RetroGame.DEBUG)
                RiotGuardWall.DrawDebug(spriteBatch);
            spriteBatch.End();

            // Draw on test buffer
            GraphicsDevice.SetRenderTarget(testRenderTarget2);
            GraphicsDevice.Clear(Color.Transparent);

            //HUD
            HUD.PreDraw();

            Effect testEffect = Effects.RewindRandomStatic;

            //testEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.Milliseconds);

            //testEffect.CurrentTechnique = testEffect.Techniques["CreateStatic"];
            //testEffect.Parameters["randomSeed"].SetValue((float)RetroGame.rand.NextDouble());
            //testEffect.Parameters["numLinesOfStatic"].SetValue(4);
            //testEffect.Parameters["staticPositions"].SetValue(new float[3] { (testEffectRadius * 2) % 1f, 0.5f, 0.7f });
            //testEffect.Parameters["staticThicknesses"].SetValue(new float[3] { 0.2f, testEffectRadius, 0.2f });
            //testEffect.Parameters["fadeIntensity"].SetValue(0.5f);

            //testEffect.CurrentTechnique = testEffect.Techniques["DistortRight"];
            //testEffect.Parameters["waveFrequency"].SetValue(0.3f);
            //testEffect.Parameters["waveAmplitude"].SetValue(testEffectRadius * 5);
            //testEffect.Parameters["granularity"].SetValue(0.5f);
            //testEffect.Parameters["waveOffset"].SetValue(-0.5f);
            //testEffect.Parameters["phaseOffset"].SetValue(testEffectRadius * 2);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            //DepthStencilState.None, RasterizerState.CullCounterClockwise,
            //testEffect, Matrix.Identity);
            //spriteBatch.Draw(shaderRenderTarget, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight),
            //    null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            //spriteBatch.End();

            //GraphicsDevice.SetRenderTarget(testRenderTarget);
            //GraphicsDevice.Clear(Color.Transparent);
            //testEffect.CurrentTechnique = testEffect.Techniques["Fade"];
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            //DepthStencilState.None, RasterizerState.CullCounterClockwise,
            //testEffect, Matrix.Identity);
            //spriteBatch.Draw(testRenderTarget, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight),
            //    null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            //spriteBatch.End();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteFont fontDebug = RetroGame.FONT_DEBUG;

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
            spriteBatch.Draw(testRenderTarget2, Vector2.Zero, Color.Lerp(Color.White, Color.Transparent, 0.0f));
            spriteBatch.End();

            // Draw on HUD/UI area using spriteBatchHUD
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            HUD.Draw(spriteBatchHUD);

            if (RetroGame.DEBUG)
            {
                spriteBatchHUD.DrawString(fontDebug, "Time to drill: " + RiotGuardWall.timeToDrill + "\nDrilling time:" + RiotGuardWall.drillingTime + "\nMusic Test Keys\n[HNJMK,.]", new Vector2(350, HUD.hudHeight + 200), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                for (int i = 0; i < levelManager.heroes.Length; i++)
                {
                    Hero hero = levelManager.heroes[i];
                    spriteBatchHUD.DrawString(fontDebug, "(" + hero.levelX + ", " + hero.levelY + ")" + "\nCell: " + levelManager.levels[hero.levelX, hero.levelY].cellName, new Vector2(300, HUD.hudHeight + 90 + i * 45), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                }
            }

            if (drawVignette)
                Vignette.Draw(spriteBatchHUD, vignetteColor, vignetteIntensity);
            if (RetroGame.DEBUG)
            {
                levelManager.DrawDebugHUD(spriteBatchHUD);
            }
            spriteBatchHUD.End();
        }

        public override void Dispose()
        {
            spriteBatch.Dispose();
            spriteBatchHUD.Dispose();
            shaderRenderTarget.Dispose();
            textureRandom.Dispose();
            testRenderTarget.Dispose();
            testRenderTarget2.Dispose();
        }
    }
}
