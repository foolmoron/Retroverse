using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class PauseScreen : MenuScreen
    {
        public const float STATIC_TRANSITION_TIME = 1f;

        public int activePlayerFolder;
        public const float FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN = 1.5f;
        public const float FOLDER_SCREEN_POSITION_RELATIVE_SHOWN = 0.5f;
        public const float FOLDER_ROTATION_HIDDEN = 0f;
        public const float FOLDER_ROTATION_SHOWN = 0.08f;
        public const float FOLDER_SCREEN_VELOCITY_RELATIVE = 1f;
        public float folderPositionRelative = FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN;
        public Vector2 folderPosition = new Vector2(-RetroGame.screenSize.X, -RetroGame.screenSize.Y); // prevent first frame from showing in a bad position on screen
        public float folderRotation;

        public static readonly Vector2 PHOTO_POSITION_RELATIVE = new Vector2(0.12f, 0.115f);
        public static readonly Vector2 PAPER_POSITION_RELATIVE = new Vector2(0.52f, 0.14f);
        public static readonly Vector2 TAB_POSITION1_RELATIVE = new Vector2(0.70f, 0.0f);
        public static readonly Vector2 TAB_POSITION2_RELATIVE = new Vector2(0.60f, 0.0f);

        public static readonly Vector2 PHOTO_HERO_POSITION_RELATIVE = new Vector2(0.15f, 0.10f);
        public static readonly Vector2 PHOTO_HERO_SIZE_RELATIVE = new Vector2(0.70f, 0.70f);
        public static readonly Vector2 PHOTO_NAME_POSITION_RELATIVE = new Vector2(0.5f, 0.87f);
        public static readonly float PHOTO_NAME_MAXWIDTH_RELATIVE = 0.8f;
        public static readonly Color PHOTO_NAME_COLOR = Color.Black;

        //highscores
        public static Vector2 HIGHSCORES_POS = new Vector2(0.55f, 0.525f);
        public static float HIGHSCORES_SCALE = 1.0f;

        public readonly MenuOptions pauseOptions;

        public PauseScreen(Bindings activeBindings)
            : base(activeBindings)
        {
            switch (activeBindings.PlayerIndex)
            {
                case PlayerIndex.One:
                    activePlayerFolder = Player.One;
                    break;
                case PlayerIndex.Two:
                    activePlayerFolder = Player.Two;
                    break;
            }
            Action onStoreTransition = delegate
                {
                    RetroGame.storeChargeTime = 0; 
                    RetroGame.StoreCharge = 0; 
                    options["Go to Store"] = null; 
                    RetroGame.AddScreen(new StoreScreen(null), true); 
                    Highscores.Save(); 
                    RetroGame.Save();
                };
            Action<MenuOptionAction> storeAction = delegate { SoundManager.SetMusicVolumeSmooth(0); RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.ToStatic, STATIC_TRANSITION_TIME, onStoreTransition, true)); };
            pauseOptions = new MenuOptions("Paused",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"Resume", delegate{ mode = MenuScreenMode.TransitioningOut; }},
                    {"Go to Store", (RetroGame.StoreCharge >= 1) ? storeAction : null},
                    {"Inventory", delegate{ RetroGame.AddScreen(new InventoryScreen(bindings)); }},
                    {"Settings", delegate { settingsMode = SettingsMode.Menu; SetMenuOptions(GetSettingsOptions()); }},
                    {"Restart", delegate { DisplayConfirmationDialog("Restart?", delegate{
                        RetroGame.PopScreen();
                        SaveGame loadedGame = RetroGame.Load(Saves.LastSaveFilename);
                        if (loadedGame != null)
                        {
                            Type[] powerupTypes = new Type[loadedGame.storePowerupTypeNames.Length];
                            for (int i = 0; i < powerupTypes.Length; i++)
                                powerupTypes[i] = Type.GetType(loadedGame.storePowerupTypeNames[i]);
                            RetroGame.AddScreen(new StoreScreen(powerupTypes), true);
                        }
                        }); }},
                    {"Quit Game", delegate { DisplayConfirmationDialog("Quit?", delegate
                        {
                            RetroGame.Reset();
                            RetroGame.PopScreen(); 
                            RetroGame.AddScreen(new StartScreen(RetroGame.getHeroes()[0].bindings), true);
                            ((StartScreen)RetroGame.TopScreen).folderPositionRelative = StartScreen.FOLDER_SCREEN_POSITION_RELATIVE_SHOWN;
                        }); }},
                }
                , 0);
            SetMenuOptions(pauseOptions);
        }

        public override void OnScreenSizeChanged()
        {
            base.OnScreenSizeChanged();
            folderPosition = new Vector2(0.5f, folderPositionRelative) * RetroGame.screenSize;
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            base.OnInputAction(action, pressedThisFrame);
            if (settingsMode == SettingsMode.None && RetroGame.NUM_PLAYERS > 1 && pressedThisFrame)
            {
                switch (action)
                {
                    case InputAction.Left:
                        activePlayerFolder = (activePlayerFolder + 1) % RetroGame.NUM_PLAYERS;
                        SoundManager.PlaySoundOnce("ButtonForward");
                        break;
                    case InputAction.Right:
                        activePlayerFolder--;
                        if(activePlayerFolder < 0)
                            activePlayerFolder = RetroGame.NUM_PLAYERS - 1;
                        SoundManager.PlaySoundOnce("ButtonForward");
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float seconds = gameTime.getSeconds();
            switch(mode)
            {
                case MenuScreenMode.TransitioningOut:
                case MenuScreenMode.TransitioningIn:
                    {
                        if (mode == MenuScreenMode.TransitioningIn)
                            folderPositionRelative = MathHelper.Clamp(folderPositionRelative -= FOLDER_SCREEN_VELOCITY_RELATIVE * seconds, FOLDER_SCREEN_POSITION_RELATIVE_SHOWN, FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN);
                        else if (mode == MenuScreenMode.TransitioningOut)
                            folderPositionRelative = MathHelper.Clamp(folderPositionRelative += FOLDER_SCREEN_VELOCITY_RELATIVE * seconds, FOLDER_SCREEN_POSITION_RELATIVE_SHOWN, FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN);
                        folderPosition = new Vector2(0.5f, folderPositionRelative) * RetroGame.screenSize;
                                
                        float interp = (FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN - folderPositionRelative) / (FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN - FOLDER_SCREEN_POSITION_RELATIVE_SHOWN);
                        folderRotation = FOLDER_ROTATION_HIDDEN * (1 - interp) + FOLDER_ROTATION_SHOWN * interp;
                
                        if (interp == 1)
                        {
                            mode = MenuScreenMode.Active;
                        }
                        else if (interp == 0)
                        {
                            RetroGame.PopScreen();
                            SoundManager.ResumeLoopingSounds();
                        }
                    }
                    break;
            }
        }

        public override void PreDraw(GameTime gameTime)
        {
            Hero hero = RetroGame.getHeroes()[activePlayerFolder];
            for(int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                DrawTab(i);
            }
            DrawPhoto();
            spriteBatchHUD.Begin();
            Point pos = new Point((int)(PHOTO_HERO_POSITION_RELATIVE.X * photoRenderTarget.Width), (int)(PHOTO_HERO_POSITION_RELATIVE.Y * photoRenderTarget.Height));
            Point size = new Point((int)(PHOTO_HERO_SIZE_RELATIVE.X * photoRenderTarget.Width), (int)(PHOTO_HERO_SIZE_RELATIVE.Y * photoRenderTarget.Height));
            spriteBatchHUD.Draw(hero.getTexture(), new Rectangle(pos.X, pos.Y, size.X, size.Y), null, hero.color, 0, Vector2.Zero, SpriteEffects.None, 0);
            string nameString = "#" + hero.prisonerID.ToString("0000") + " " + hero.prisonerName;
            Vector2 nameDims = RetroGame.FONT_PIXEL_LARGE.MeasureString(nameString);
            float nameScale = (PHOTO_NAME_MAXWIDTH_RELATIVE * photoRenderTarget.Width) / nameDims.X;
            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, nameString, new Vector2(PHOTO_NAME_POSITION_RELATIVE.X * photoRenderTarget.Width, PHOTO_NAME_POSITION_RELATIVE.Y * photoRenderTarget.Height), PHOTO_NAME_COLOR, 0, nameDims / 2, nameScale, SpriteEffects.None, 0);
            spriteBatchHUD.End();
            DrawPaper();
            DrawFolder();
            DrawStatic();

            GraphicsDevice.SetRenderTarget(finalRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Vector2 texSize = new Vector2(finalRenderTarget.Width, finalRenderTarget.Height);
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            if (RetroGame.NUM_PLAYERS == 2)
            {
                if (activePlayerFolder != Player.Two)
                    spriteBatchHUD.Draw(tab2RenderTarget, TAB_POSITION2_RELATIVE * texSize, Color.White);
                else
                    spriteBatchHUD.Draw(tab1RenderTarget, TAB_POSITION1_RELATIVE * texSize, Color.White);
            }
            spriteBatchHUD.Draw(folderRenderTarget, Vector2.Zero, Color.White);
            if (settingsMode != SettingsMode.Bindings)
            {
                spriteBatchHUD.Draw(photoRenderTarget, PHOTO_POSITION_RELATIVE * texSize, Color.White);
            }
            spriteBatchHUD.Draw(paperRenderTarget, PAPER_POSITION_RELATIVE * texSize, Color.White);
            if (RetroGame.NUM_PLAYERS == 2 && activePlayerFolder == Player.Two)
                spriteBatchHUD.Draw(tab2RenderTarget, TAB_POSITION2_RELATIVE * texSize, Color.White);
            else
                spriteBatchHUD.Draw(tab1RenderTarget, TAB_POSITION1_RELATIVE * texSize, Color.White);
            DrawMenu();
            if (settingsMode == SettingsMode.None || settingsMode == SettingsMode.Menu)
            {
                HeroInfo.Draw(hero, spriteBatchHUD, texSize);
                Highscores.Draw((RetroGame.NUM_PLAYERS == 1) ? Highscores.DrawMode.Solo : Highscores.DrawMode.Coop, spriteBatchHUD, HIGHSCORES_POS * texSize, HIGHSCORES_SCALE);
            }
            spriteBatchHUD.End();
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 screenSize = RetroGame.screenSize;
            float hudScale = (HUD.hudScale + 1) / 2;
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatchHUD.Draw(staticRenderTarget, Vector2.Zero, null, Color.White.withAlpha(180), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatchHUD.Draw(finalRenderTarget, folderPosition, null, Color.White, folderRotation, FOLDER_ORIGIN, FOLDER_BASE_SCALE * hudScale, SpriteEffects.None, 0);
            float versionHeight = RetroGame.FONT_DEBUG.MeasureString(RetroGame.VERSION).Y;
            spriteBatchHUD.DrawString(RetroGame.FONT_DEBUG, RetroGame.VERSION, new Vector2(0, screenSize.Y - versionHeight), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            if (RetroGame.DEBUG)
            {
                string levelsString = RetroGame.TopLevelManagerScreen.levelManager.levels[RetroGame.getMainLiveHero().levelX, RetroGame.getMainLiveHero().levelY].fragmentGrid[0, 0].name + "|" + RetroGame.TopLevelManagerScreen.levelManager.levels[RetroGame.getMainLiveHero().levelX, RetroGame.getMainLiveHero().levelY].fragmentGrid[1, 0].name + "\n" +
                                         RetroGame.TopLevelManagerScreen.levelManager.levels[RetroGame.getMainLiveHero().levelX, RetroGame.getMainLiveHero().levelY].fragmentGrid[0, 1].name + "|" + RetroGame.TopLevelManagerScreen.levelManager.levels[RetroGame.getMainLiveHero().levelX, RetroGame.getMainLiveHero().levelY].fragmentGrid[1, 1].name;
                spriteBatchHUD.DrawString(RetroGame.FONT_DEBUG, levelsString, new Vector2(0, screenSize.Y - (versionHeight * 3)), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
            spriteBatchHUD.End();
        }
    }
}
