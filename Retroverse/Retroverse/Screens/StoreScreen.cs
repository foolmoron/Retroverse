using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class StoreScreen : LevelManagerScreen
    {
        public const float BACKGROUND_MUSIC_VOLUME = 0.5f;
        public const float STATIC_TRANSITION_TIME = 1f;

        public SpriteBatch spriteBatch;
        public SpriteBatch spriteBatchHUD;
        public StoreLevel storeLevel;

        public RenderTarget2D staticBorderRenderTarget;

        internal class HeroSaveState
        {
            internal Vector2 position;
            internal Direction direction;

            internal HeroSaveState(Hero hero)
            {
                position = hero.position;
                direction = hero.direction;
            }
        }
        private HeroSaveState[] heroSaveStates = new HeroSaveState[RetroGame.NUM_PLAYERS];

        public StoreScreen(IList<Type> powerupTypesToUse)
        {
            DrawPreviousScreen = false;
            levelManager = new LevelManager();
            for (int i = 0; i < RetroGame.getHeroes().Length; i++)
            {
                if (RetroGame.getHeroes()[i].Alive)
                    heroSaveStates[i] = new HeroSaveState(RetroGame.getHeroes()[i]);
            }
            levelManager.heroes = RetroGame.getHeroes();
            Point startingLevel = LevelManager.STARTING_LEVEL;
            levelManager.Initialize(RetroGame.NUM_PLAYERS, false, RetroGame.StoreLevelFragment, startingLevel);
            storeLevel = new StoreLevel(levelManager, RetroGame.StoreLevelFragment, powerupTypesToUse, startingLevel.X, startingLevel.Y);
            levelManager.putPremadeLevelAt(storeLevel, startingLevel.X, startingLevel.Y);
        }

        public Point GetFarthestHeroLevel()
        {
            int farthestHeroIndex = 0;
            for (int i = 0; i < heroSaveStates.Length; i++)
            {
                if ((int)(heroSaveStates[i].position.X / Level.TEX_SIZE) > (int)(heroSaveStates[farthestHeroIndex].position.X / Level.TEX_SIZE))
                    farthestHeroIndex = i;
            }
            return new Point((int)(heroSaveStates[farthestHeroIndex].position.X / Level.TEX_SIZE), (int)(heroSaveStates[farthestHeroIndex].position.Y / Level.TEX_SIZE));
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.FromStatic, STATIC_TRANSITION_TIME, null), true); // always make static transition in to store screen
            GraphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatchHUD = new SpriteBatch(graphicsDevice);
            staticBorderRenderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            History.CancelRevert();
        }

        public override void OnScreenSizeChanged()
        {
            staticBorderRenderTarget.Dispose();
            staticBorderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public override void OnPaused()
        {
            PauseScreen pause = ((PauseScreen)RetroGame.TopScreen);
            Dictionary<string, Action<MenuOptionAction>> newOptions = new Dictionary<string, Action<MenuOptionAction>>();
            for (int i = 0; i < pause.options.Count; i++)
            {
                if (pause.options[i].Key == "Go to Store")
                {
                    newOptions.Add("Leave Store", delegate { leaveStore(); });
                }
                else if (pause.options[i].Key == "Restart") { }
                else if (pause.options[i].Key == "Quit Game") { }
                else
                {
                    newOptions.Add(pause.options[i].Key, pause.options[i].Value);
                }
            }
            pause.SetMenuOptions(new MenuOptions(pause.options.Title, newOptions, pause.options.BackAction));
        }

        public void leaveStore()
        {
            Action onTransitionAction = delegate
            {
                HeroSaveState firstNonNullHeroSaveState = heroSaveStates.First(state => state != null);
                for(int i = 0; i < heroSaveStates.Length; i++)
                {
                    HeroSaveState heroSaveState = heroSaveStates[i];
                    Hero hero = RetroGame.getHeroes()[i];
                    if (heroSaveState != null)
                    {
                        hero.position = heroSaveState.position;
                        hero.direction = heroSaveState.direction;
                        hero.updateCurrentLevelAndTile();
                    }
                    else
                    {
                        hero.position = firstNonNullHeroSaveState.position;
                        hero.direction = firstNonNullHeroSaveState.direction;
                        hero.updateCurrentLevelAndTile();
                    } 
                }
                RetroGame.PopScreen(true); // pause screen
                RetroGame.PopScreen(true); // store screen
                RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.FromStatic, STATIC_TRANSITION_TIME, null), true);
                SoundManager.SetMusicVolumeSmooth(MenuScreen.BACKGROUND_MUSIC_VOLUME);
                SoundManager.PlaySoundAsMusic("MainTheme");
            };
            RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.ToStatic, STATIC_TRANSITION_TIME, onTransitionAction), true);
            SoundManager.SetMusicVolumeSmooth(0);
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            // LevelManager, Hero, and other components handle the inputs for this screen
        }

        public override void Update(GameTime gameTime)
        {
            if (SoundManager.TargetVolume != BACKGROUND_MUSIC_VOLUME)
                SoundManager.SetMusicVolumeSmooth(BACKGROUND_MUSIC_VOLUME);
            SoundManager.PlaySoundAsMusic("StoreTheme"); //ignores if already playing

            storeLevel.Update(gameTime);
            levelManager.UpdateArena(gameTime);
            HUD.Update(gameTime);
        }

        public override void PreDraw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(staticBorderRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Effect staticEffect = Effects.StaticWithAlpha;
            staticEffect.Parameters["AlphaTexture"].SetValue(TextureManager.Get("staticstorealpha"));
            staticEffect.Parameters["randomSeed"].SetValue((float)RetroGame.rand.NextDouble());
            staticEffect.Parameters["whiteness"].SetValue(MenuScreen.STATIC_WHITENESS);
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise,
            staticEffect, Matrix.Identity);
            spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle(0, 0, staticBorderRenderTarget.Width, staticBorderRenderTarget.Height), Color.White);
            spriteBatchHUD.End();

            //HUD
            HUD.PreDraw();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise,
                null, levelManager.getViewMatrix());
            if (RetroGame.drawLevelDebugTextures)
                levelManager.DrawDebug(spriteBatch);
            levelManager.Draw(spriteBatch);
            spriteBatch.End();

            // Draw on HUD/UI area using spriteBatchHUD
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatchHUD.Draw(staticBorderRenderTarget, Vector2.Zero, Color.White);
            HUD.Draw(spriteBatchHUD);

            SpriteFont fontDebug = RetroGame.FONT_DEBUG;
            if (RetroGame.DEBUG)
            {
                spriteBatchHUD.DrawString(fontDebug, "Time to drill: " + RiotGuardWall.timeToDrill + "\nMusic Test Keys\n[HNJMK,.]", new Vector2(350, HUD.hudHeight + 200), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                for (int i = 0; i < levelManager.heroes.Length; i++)
                {
                    Hero hero = levelManager.heroes[i];
                    spriteBatchHUD.DrawString(fontDebug, "Hero #" + hero.prisonerID.ToString("0000") + " " + hero.prisonerName + "\nCell: " + levelManager.levels[hero.levelX, hero.levelY].cellName, new Vector2(300, HUD.hudHeight + 90 + i * 45), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
                }
                levelManager.DrawDebugHUD(spriteBatchHUD);
            }
            spriteBatchHUD.End();
        }

        public override void Dispose()
        {
            spriteBatch.Dispose();
            staticBorderRenderTarget.Dispose();
        }
    }
}
