using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class MenuScreen : Screen
    {
        public  const float BACKGROUND_MUSIC_VOLUME = 1.0f;
        public Bindings bindings;

        public enum MenuScreenMode { TransitioningIn, TransitioningOut, Active };
        public enum SettingsMode { None, Menu, Bindings, Sound, ScreenSize, Continue, Setup, Credits };
        public MenuScreenMode mode = MenuScreenMode.TransitioningIn;
        public SettingsMode settingsMode = SettingsMode.None;
        public bool TransitionOutEnabled { get; protected set; }

        public RenderTarget2D tab1RenderTarget;
        public RenderTarget2D tab2RenderTarget;
        public RenderTarget2D photoRenderTarget;
        public RenderTarget2D paperRenderTarget;
        public RenderTarget2D folderRenderTarget;
        public RenderTarget2D staticRenderTarget;
        public RenderTarget2D finalRenderTarget;
        public SpriteBatch spriteBatchHUD;

        //tab drawing
        public static readonly Vector2 TAB_ID_POSITION_RELATIVE = new Vector2(0.5f, 0.5f);
        public static readonly Color TAB_ID_COLOR = Color.Black;
        public static readonly float TAB_ID_WIDTH_RELATIVE = 0.6f;

        public Texture2D folderTex;
        public Texture2D tabTex;
        public Texture2D photoTex;
        public Texture2D paperTex;
        public static Vector2 FOLDER_ORIGIN;
        public const float FOLDER_BASE_SCALE = 0.65f;
        public const float STATIC_WHITENESS = 0.75f;

        public MenuOptions options;
        public int currentMenuIndex = 0;

        private MenuOptions preConfirmationDialogOptions;
        private MenuOptions preSettingsOptionsOptions;

        protected MenuOptions settingsOptions;
        protected MenuOptions screenSizeOptions;

        //bindings
        private PlayerIndex currentPlayerBindingsMenu;
        private List<Bindings.Binding> customBindingsToModify;
        private bool usesCustomBindings;
        public const float KEY_BINDINGS_XPOS = 0.7f;
        public const float KEY_BINDINGS_SCALE = 0.75f;
        public const float KEY_TEXT_XPOS = 0.60f;
        public const float KEY_TEXT_SCALE = 1.0f;
        public static readonly Color KEY_BINDINGS_COLOR = Color.Black;
        public const float XBOX_BINDINGS_XPOS = 0.825f;
        public const float XBOX_BINDINGS_SCALE = 1.4f;
        private string bindingsMenuInstructions;
        public static readonly Vector2 BINDINGS_INSTRUCTIONS_POS = new Vector2(0.55f, 0.2f);
        public const float BINDINGS_INSTRUCTIONS_SCALE = 1.0f;
        public Color bindingsMenuInstructionsColor;
        public static readonly Color BINDINGS_INSTRUCTIONS_NOTIFY_COLOR = Color.Cyan;
        public static readonly Color BINDINGS_INSTRUCTIONS_ERROR_COLOR = Color.Red;
        public static readonly Color BINDINGS_INSTRUCTIONS_IDLE_COLOR = Color.Black;
        public const float BINDINGS_INSTRUCTIONS_NOTIFY_TIME = 1f; //secs
        public float bindingsMenuInstructionsNotifyTime = BINDINGS_INSTRUCTIONS_NOTIFY_TIME; //secs
        public bool bindingsMenuInstructionsError = false;

        //sound options
        protected MenuOptions soundOptions;
        public const float VOLUME_SETTINGS_STEP = 0.1f;
        public float previousMasterVolume;
        public float previousMusicVolume;
        public float previousSoundVolume;
        public const float VOLUME_BAR_XPOS = 0.60f;
        public const float VOLUME_BAR_WIDTH = 0.25f;
        public const float VOLUME_ICON_XPOS = 0.575f;
        public RenderTarget2D soundRenderTarget;
        public Texture2D soundLoudTex;
        public Texture2D soundMuteTex;
        public static readonly Color VOLUME_BAR_COLOR_OUTER = Color.Black;
        public static readonly Color VOLUME_BAR_COLOR_INNER = Color.Cyan;

        public static readonly Vector2 MENU_BOTTOM_OPTION_POSITION = new Vector2(0.11f, 0.85f);
        public const float MENU_POSITION_VERTICAL_STEP = 0.05f;
        public static readonly Color MENU_COLOR_UNAVAILABLE = Color.Gray;
        public static readonly Color MENU_COLOR_UNSELECTED = Color.Black;
        public static readonly Color MENU_COLOR_SELECTED = Color.White;
        public const float MENU_OPTIONS_TEXT_SCALE = 1;
        public static readonly Color MENU_COLOR_TITLE = Color.Cyan;
        public const float MENU_OPTIONS_TITLE_SCALE = 1f;
        
        public const float ARROW_DISTANCE_MAX = 0.045f; //relative
        public const float ARROW_DISTANCE_MIN = 0.025f;
        public const float ARROW_DISTANCE_VELOCITY = 0.06f;
        public int arrowVelocityModifier = 1;
        public float arrowDistance = ARROW_DISTANCE_MIN;

        public MenuScreen(Bindings activeBindings)
        {
            DrawPreviousScreen = true;
            mode = MenuScreenMode.TransitioningIn;
            TransitionOutEnabled = true;

            bindings = activeBindings;

            settingsOptions = new MenuOptions("Settings",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"<P1 Bindings>", bindingsMenuOptionDelegate},
                    {"Sound", delegate{ settingsMode = SettingsMode.Sound; SetMenuOptions(soundOptions); }},
                    {"Screen Size", delegate { settingsMode = SettingsMode.ScreenSize; SetMenuOptions(screenSizeOptions); }},
                    {"Reset Scores", delegate{ DisplayConfirmationDialog("Reset Scores?", delegate { RetroGame.ResetScores(); SetMenuOptions(settingsOptions); }); } },
                    {"Back", null }, //set dynamically
                }
                , "Back");

            soundOptions = new MenuOptions("Sound",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"<Master>", delegate(MenuOptionAction action)
                        {
                            switch (action)
	                        {
                                case MenuOptionAction.Click:
	                                if(SoundManager.MasterVolume != 0)
	                                {
                                        previousMasterVolume = SoundManager.MasterVolume;
                                        SoundManager.SetMasterVolume(0);
	                                }
	                                else
	                                    SoundManager.SetMasterVolume(previousMasterVolume);
	                                break;
	                            case MenuOptionAction.Left:
	                                SoundManager.SetMasterVolume(SoundManager.MasterVolume - VOLUME_SETTINGS_STEP);
                                    break;
                                case MenuOptionAction.Right:
                                    SoundManager.SetMasterVolume(SoundManager.MasterVolume + VOLUME_SETTINGS_STEP);
                                    break;
	                        }
                            RetroGame.SaveConfig(); 
                        }},
                    {"<Music>", delegate(MenuOptionAction action)
                        {
                            switch (action)
                            {
                                case MenuOptionAction.Click:
                                    if (SoundManager.MusicMasterVolume != 0)
                                    {
                                        previousMusicVolume = SoundManager.MusicMasterVolume;
                                        SoundManager.SetMusicMasterVolume(0);
                                    }
                                    else
                                        SoundManager.SetMusicMasterVolume(previousMusicVolume);
                                    break;
                                case MenuOptionAction.Left:
	                                SoundManager.SetMusicMasterVolume(SoundManager.MusicMasterVolume - VOLUME_SETTINGS_STEP);
                                    break;
                                case MenuOptionAction.Right:
                                    SoundManager.SetMusicMasterVolume(SoundManager.MusicMasterVolume + VOLUME_SETTINGS_STEP);
                                    break;
	                        }
                            RetroGame.SaveConfig(); 
                        }},
                    {"<Sounds>", delegate(MenuOptionAction action)
                        {
                            switch (action)
                            {
                                case MenuOptionAction.Click:
                                    if (SoundManager.SoundMasterVolume != 0)
                                    {
                                        previousSoundVolume = SoundManager.SoundMasterVolume;
                                        SoundManager.SetSoundMasterVolume(0);
                                    }
                                    else
                                        SoundManager.SetSoundMasterVolume(previousSoundVolume);
                                    break;
                                case MenuOptionAction.Left:
	                                SoundManager.SetSoundMasterVolume(SoundManager.SoundMasterVolume - VOLUME_SETTINGS_STEP);
                                    break;
                                case MenuOptionAction.Right:
                                    SoundManager.SetSoundMasterVolume(SoundManager.SoundMasterVolume + VOLUME_SETTINGS_STEP);
                                    break;
	                        }
                            RetroGame.SaveConfig(); 
                        }},
                    {"Back", delegate{ settingsMode = SettingsMode.Menu; SetMenuOptions(settingsOptions); }},
                }
                , "Back");
            screenSizeOptions = new MenuOptions("Screen Size",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"Small", delegate{ RetroGame.SetScreenSize(ScreenSize.Small); RetroGame.SaveConfig(); }},
                    {"Medium", delegate{ RetroGame.SetScreenSize(ScreenSize.Medium); RetroGame.SaveConfig(); }},
                    {"Large", delegate{ RetroGame.SetScreenSize(ScreenSize.Large); RetroGame.SaveConfig(); }},
                    {"Huge", delegate{ RetroGame.SetScreenSize(ScreenSize.Huge); RetroGame.SaveConfig(); }},
                    {"Back", delegate{ settingsMode = SettingsMode.Menu;  SetMenuOptions(settingsOptions); }},
                }
                , 4);
        }

        public void SetMenuOptions(MenuOptions options)
        {
            if(options == null)
                throw new ArgumentNullException("options");
            this.options = options;
            currentMenuIndex = 0;
        }

        public void bindingsMenuOptionDelegate(MenuOptionAction action)
        {
            switch (action)
            {
                case MenuOptionAction.Click:
                    settingsMode = SettingsMode.Bindings;
                    bool p1 = options[0].Key == "<P1 Bindings>";
                    bindingsMenuInstructions = "Select an action to modify\nbindings for that action";
                    usesCustomBindings = Bindings.USE_CUSTOM_BINDINGS[p1 ? PlayerIndex.One : PlayerIndex.Two];
                    customBindingsToModify = Bindings.CUSTOM_BINDINGS[p1 ? PlayerIndex.One : PlayerIndex.Two];
                    currentPlayerBindingsMenu = p1 ? PlayerIndex.One : PlayerIndex.Two;
                    MenuOptions bindingsMenu = new MenuOptions(p1 ? "P1 Bindings" : "P2 Bindings",
                        new Dictionary<string, Action<MenuOptionAction>>()
	                    {
                            { usesCustomBindings ? "<Custom>" : "<Default>", 
                                delegate(MenuOptionAction action2)
	                            {
                                    switch (action2)
	                                {
	                                    case MenuOptionAction.Click:
	                                    case MenuOptionAction.Left:
	                                    case MenuOptionAction.Right:
	                                        if(usesCustomBindings)
	                                        {
	                                            options[0] = new KeyValuePair<string, Action<MenuOptionAction>>("<Default>", options[0].Value);
	                                            options.SetEnabled(false, 1, 9);
	                                            usesCustomBindings = false;
	                                        }
	                                        else
	                                        {
                                                options[0] = new KeyValuePair<string, Action<MenuOptionAction>>("<Custom>", options[0].Value);
                                                options.SetEnabled(true, 1, 9);
	                                            usesCustomBindings = true;
	                                        }
	                                        break;
	                                }
	                            }
                            },
                            {"  Up", getSetBindingDelegate(InputAction.Up)},
                            {"  Down", getSetBindingDelegate(InputAction.Down)},
                            {"  Left", getSetBindingDelegate(InputAction.Left)},
                            {"  Right", getSetBindingDelegate(InputAction.Right)},
                            {"  Action1", getSetBindingDelegate(InputAction.Action1)},
                            {"  Action2", getSetBindingDelegate(InputAction.Action2)},
                            {"  Action3", getSetBindingDelegate(InputAction.Action3)},
                            {"  Action4", getSetBindingDelegate(InputAction.Action4)},
                            {"  Start", getSetBindingDelegate(InputAction.Start)},
                            {"Apply", 
                                delegate
                                {
                                    Bindings.USE_CUSTOM_BINDINGS[p1 ? PlayerIndex.One : PlayerIndex.Two] = usesCustomBindings;
                                    Bindings.CUSTOM_BINDINGS[p1? PlayerIndex.One : PlayerIndex.Two] = customBindingsToModify;
                                    if (p1 && RetroGame.getHeroes().Length > 0)
                                    {
                                        if (usesCustomBindings)
                                            RetroGame.getHeroes()[0].bindings.setToCustom();
                                        else
                                            RetroGame.getHeroes()[0].bindings.setToDefault();
                                    }
                                    else if (RetroGame.getHeroes().Length > 1)
                                    {
                                        if (usesCustomBindings)
                                            RetroGame.getHeroes()[1].bindings.setToCustom();
                                        else
                                            RetroGame.getHeroes()[1].bindings.setToDefault();
                                    }
                                    RetroGame.SaveConfig(); 
                                    settingsMode = SettingsMode.Menu;
                                    SetMenuOptions(settingsOptions);
                                }
                            },
                            {"Cancel", 
                                delegate
                                { 
                                    customBindingsToModify = null; 
                                    settingsMode = SettingsMode.Menu; 
                                    SetMenuOptions(settingsOptions); 
                                }
                            },
	                    },
                        "Cancel");
                    SetMenuOptions(bindingsMenu);
	                options.SetEnabled(usesCustomBindings, 1, 9);
                    break;
                case MenuOptionAction.Left:
                case MenuOptionAction.Right:
                    if (options[0].Key == "<P1 Bindings>")
                        options[0] = new KeyValuePair<string, Action<MenuOptionAction>>("<P2 Bindings>", options[0].Value);
                    else
                        options[0] = new KeyValuePair<string, Action<MenuOptionAction>>("<P1 Bindings>", options[0].Value);
                    break;
            }
        }

        public Action<MenuOptionAction> getSetBindingDelegate(InputAction actionToBind)
        {
            return delegate
            {
                bindingsMenuInstructionsError = false;
                bindingsMenuInstructionsNotifyTime = 0;
                bindingsMenuInstructions = "Press the key or button you wish\nto bind to action [" + actionToBind + "]";
                currentBindings.onNextInputAction =
                    delegate(Keys? pressedKey, Buttons? pressedButton)
                    {
                        bindingsMenuInstructionsNotifyTime = 0;
                        if (pressedKey != null)
                        {
                            if (Bindings.BINDABLE_KEYS.Contains(pressedKey.Value))
                            {
                                for (int i = 0; i < customBindingsToModify.Count; i++)
                                {
                                    Bindings.Binding bind = customBindingsToModify[i];
                                    if (bind.Action == actionToBind)
                                    {
                                        customBindingsToModify.RemoveAt(i);
                                        for (int j = 0; j < customBindingsToModify.Count; j++)
                                        {
                                            Bindings.Binding swappingBind = customBindingsToModify[j];
                                            if (swappingBind.Key == pressedKey.Value)
                                            {
                                                customBindingsToModify.RemoveAt(j);
                                                customBindingsToModify.Add(new Bindings.Binding(swappingBind.Action, bind.Key, swappingBind.Button));
                                            }
                                        }
                                        customBindingsToModify.Add(new Bindings.Binding(bind.Action, pressedKey.Value, bind.Button));
                                        customBindingsToModify.Sort();
                                        break;
                                    }
                                }
                                bindingsMenuInstructionsError = false;
                                bindingsMenuInstructions = "Key [" + pressedKey.Value + "]\nsuccesfully bound to\naction [" + actionToBind + "]";
                                SoundManager.PlaySoundOnce("ButtonForward");
                            }
                            else
                            {
                                bindingsMenuInstructionsError = true;
                                bindingsMenuInstructions = "Key [" + pressedKey.Value + "]\ncannot be bound to actions";
                                SoundManager.PlaySoundOnce("ButtonFailure");
                            }
                            return;
                        }
                        if (pressedButton != null)
                        {
                            if (new[] { InputAction.Up, InputAction.Down, InputAction.Left, InputAction.Right }.Contains(actionToBind))
                            {
                                bindingsMenuInstructionsError = true;
                                bindingsMenuInstructions = "Cannot rebind action [" + actionToBind + "]\non Xbox controller";
                                SoundManager.PlaySoundOnce("ButtonFailure");
                            }
                            else if (Bindings.BINDABLE_BUTTONS.Contains(pressedButton.Value))
                            {
                                for (int i = 0; i < customBindingsToModify.Count; i++)
                                {
                                    Bindings.Binding bind = customBindingsToModify[i];
                                    if (bind.Action == actionToBind)
                                    {
                                        customBindingsToModify.RemoveAt(i);
                                        for (int j = 0; j < customBindingsToModify.Count; j++)
                                        {
                                            Bindings.Binding swappingBind = customBindingsToModify[j];
                                            if (swappingBind.Button == pressedButton.Value)
                                            {
                                                customBindingsToModify.RemoveAt(j);
                                                customBindingsToModify.Add(new Bindings.Binding(swappingBind.Action, swappingBind.Key, bind.Button));
                                            }
                                        }
                                        customBindingsToModify.Add(new Bindings.Binding(bind.Action, bind.Key, pressedButton.Value));
                                        customBindingsToModify.Sort();
                                        break;
                                    }
                                }
                                bindingsMenuInstructionsError = false;
                                bindingsMenuInstructions = "Button [" + pressedButton.Value + "]\nsuccesfully bound to\naction [" + actionToBind + "]";
                                SoundManager.PlaySoundOnce("ButtonForward");
                            }
                            else
                            {
                                bindingsMenuInstructionsError = true;
                                bindingsMenuInstructions = "Button [" + pressedButton.Value + "]\ncannot be bound to actions";
                                SoundManager.PlaySoundOnce("ButtonFailure");
                            }
                        }
                    }; 
            };
        }

        public void DisplayConfirmationDialog(string title, Action<MenuOptionAction> onConfirmAction)
        {
            DisplayConfirmationDialog(title, "Yes", "No", onConfirmAction);
        }

        public void DisplayConfirmationDialog(string title, string positiveOption, string negativeOption, Action<MenuOptionAction> onConfirmAction)
        {
            preConfirmationDialogOptions = options;
            SetMenuOptions(new MenuOptions(title, new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {negativeOption, delegate { SetMenuOptions(preConfirmationDialogOptions); }},
                    {positiveOption, onConfirmAction},
                }, negativeOption));
        }

        public MenuOptions GetSettingsOptions()
        {
            preSettingsOptionsOptions = options;
            settingsOptions["Back"] = delegate { settingsMode = SettingsMode.None; SetMenuOptions(preSettingsOptionsOptions); };
            return settingsOptions;
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);

            tabTex = TextureManager.Get("menutab");
            photoTex = TextureManager.Get("menuphoto");
            paperTex = TextureManager.Get("menupaper");
            folderTex = TextureManager.Get("pausescreen");
            FOLDER_ORIGIN = new Vector2(folderTex.Width / 2f, folderTex.Height / 2f);
            tab1RenderTarget = new RenderTarget2D(GraphicsDevice, tabTex.Width, tabTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
            tab2RenderTarget = new RenderTarget2D(GraphicsDevice, tabTex.Width, tabTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
            photoRenderTarget = new RenderTarget2D(GraphicsDevice, photoTex.Width, photoTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
            paperRenderTarget = new RenderTarget2D(GraphicsDevice, paperTex.Width, paperTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
            folderRenderTarget = new RenderTarget2D(GraphicsDevice, folderTex.Width, folderTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
            staticRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            finalRenderTarget = new RenderTarget2D(GraphicsDevice, folderTex.Width, folderTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);

            soundLoudTex = TextureManager.Get("soundloud");
            soundMuteTex = TextureManager.Get("soundmute");
            soundRenderTarget = new RenderTarget2D(GraphicsDevice, folderTex.Width, folderTex.Height,
                false, SurfaceFormat.Color, DepthFormat.None);
        }

        public override void OnScreenSizeChanged()
        {
            staticRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            if (mode == MenuScreenMode.Active)
            {
                if (pressedThisFrame)
                {
                    switch (action)
                    {
                        case InputAction.Action1:
                            if (options.IsEnabled(currentMenuIndex))
                            {
                                arrowVelocityModifier = 1;
                                arrowDistance = ARROW_DISTANCE_MIN;
                                SoundManager.PlaySoundOnce("ButtonForward");
                                options[currentMenuIndex].Value(MenuOptionAction.Click);
                            }
                            else
                            {
                                SoundManager.PlaySoundOnce("ButtonFailure");
                            }
                            break;
                        case InputAction.Left:
                            if (options.IsEnabled(currentMenuIndex) && MenuOptions.IsLeftRightAction(options[currentMenuIndex]))
                            {
                                arrowVelocityModifier = 1;
                                arrowDistance = ARROW_DISTANCE_MIN;
                                SoundManager.PlaySoundOnce("ButtonForward");
                                options[currentMenuIndex].Value(MenuOptionAction.Left);
                            }
                            break;
                        case InputAction.Right:
                            if (options.IsEnabled(currentMenuIndex) && MenuOptions.IsLeftRightAction(options[currentMenuIndex]))
                            {
                                arrowVelocityModifier = 1;
                                arrowDistance = ARROW_DISTANCE_MIN;
                                SoundManager.PlaySoundOnce("ButtonForward");
                                options[currentMenuIndex].Value(MenuOptionAction.Right);
                            }
                            break;
                        case InputAction.Action2:
                            if (options.BackAction != null)
                            {
                                SoundManager.PlaySoundOnce("ButtonBack");
                                options.BackAction(MenuOptionAction.Click);
                            }
                            break;
                        case InputAction.Start:
                        case InputAction.Escape:
                            if (TransitionOutEnabled)
                            {
                                SoundManager.PlaySoundOnce("ButtonBack");
                                mode = MenuScreenMode.TransitioningOut;
                            }
                            break;
                        case InputAction.Up:
                            arrowVelocityModifier = 1;
                            arrowDistance = ARROW_DISTANCE_MIN;
                            currentMenuIndex--;
                            if (currentMenuIndex < 0)
                                currentMenuIndex = options.Count - 1;
                            break;
                        case InputAction.Down:
                            arrowVelocityModifier = 1;
                            arrowDistance = ARROW_DISTANCE_MIN;
                            currentMenuIndex = (currentMenuIndex + 1) % options.Count;
                            break;
                    }
                }
            }
            else
            {
                if (pressedThisFrame && (action == InputAction.Start || action == InputAction.Escape))
                {
                    if (mode == MenuScreenMode.TransitioningIn)
                    {
                        SoundManager.PlaySoundOnce("ButtonBack");
                        mode = MenuScreenMode.TransitioningOut;
                    }
                    else if (mode == MenuScreenMode.TransitioningOut)
                    {
                        SoundManager.PlaySoundOnce("ButtonForward");
                        mode = MenuScreenMode.TransitioningIn;
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            if (SoundManager.TargetVolume != BACKGROUND_MUSIC_VOLUME)
                SoundManager.SetMusicVolume(BACKGROUND_MUSIC_VOLUME);
            UpdateControls(bindings, gameTime);

            arrowDistance += seconds * ARROW_DISTANCE_VELOCITY * arrowVelocityModifier;
            if(arrowDistance >= ARROW_DISTANCE_MAX || arrowDistance <= ARROW_DISTANCE_MIN)
                arrowVelocityModifier *= -1;

            if (bindingsMenuInstructionsNotifyTime < BINDINGS_INSTRUCTIONS_NOTIFY_TIME)
            {
                bindingsMenuInstructionsNotifyTime += seconds;
            }
            bindingsMenuInstructionsColor = Color.Lerp(bindingsMenuInstructionsError ? BINDINGS_INSTRUCTIONS_ERROR_COLOR : BINDINGS_INSTRUCTIONS_NOTIFY_COLOR, BINDINGS_INSTRUCTIONS_IDLE_COLOR, bindingsMenuInstructionsNotifyTime / BINDINGS_INSTRUCTIONS_NOTIFY_TIME);
        }

        // no real drawing, just utility drawing of individual parts for subclasses to compose on the screen
        public override void PreDraw(GameTime gameTime) { }
        public override void Draw(GameTime gameTime) { }

        public void DrawTab(int player)
        {
            GraphicsDevice.SetRenderTarget((player == Player.Two) ? tab2RenderTarget : tab1RenderTarget);
            DrawTabFinal(player);
        }
        public void DrawTab(RenderTarget2D target, int player)
        {
            GraphicsDevice.SetRenderTarget(target);
            DrawTabFinal(player);
        }
        private void DrawTabFinal(int player)
        {
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatchHUD.Begin();
            spriteBatchHUD.Draw(tabTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (player != Player.None)
            {
                Hero hero = RetroGame.getHeroes()[player];
                string idString = "#" + hero.prisonerID.ToString("0000");
                Vector2 dims = RetroGame.FONT_PIXEL_SMALL.MeasureString(idString);
                float scale = (TAB_ID_WIDTH_RELATIVE * tab1RenderTarget.Width) / dims.X;
                spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, idString, TAB_ID_POSITION_RELATIVE * new Vector2(tab1RenderTarget.Width, tab1RenderTarget.Height), TAB_ID_COLOR, 0, dims / 2, scale, SpriteEffects.None, 0);
            }
            spriteBatchHUD.End();
        }

        public void DrawPhoto()
        {
            GraphicsDevice.SetRenderTarget(photoRenderTarget);
            DrawPhotoFinal();
        }
        public void DrawPhoto(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
            DrawPhotoFinal();
        }
        private void DrawPhotoFinal()
        {
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatchHUD.Begin();
            spriteBatchHUD.Draw(photoTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatchHUD.End();
        }

        public void DrawPaper()
        {
            GraphicsDevice.SetRenderTarget(paperRenderTarget);
            DrawPaperFinal();
        }
        public void DrawPaper(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
            DrawPaperFinal();
        }
        private void DrawPaperFinal()
        {
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatchHUD.Begin();
            spriteBatchHUD.Draw(paperTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatchHUD.End();
        }

        public void DrawFolder()
        {
            GraphicsDevice.SetRenderTarget(folderRenderTarget);
            DrawFolderFinal();
        }
        public void DrawFolder(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
            DrawFolderFinal();
        }
        private void DrawFolderFinal()
        {
            GraphicsDevice.Clear(Color.Transparent);
            Vector2 texSize = new Vector2(folderTex.Width, folderTex.Height);
            spriteBatchHUD.Begin();
            spriteBatchHUD.Draw(folderTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatchHUD.End();
        }

        public void DrawStatic()
        {
            GraphicsDevice.SetRenderTarget(staticRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Effect staticEffect = Effects.RewindRandomStatic;
            staticEffect.CurrentTechnique = staticEffect.Techniques["CreateStatic"];
            staticEffect.Parameters["randomSeed"].SetValue((float)RetroGame.rand.NextDouble());
            staticEffect.Parameters["numLinesOfStatic"].SetValue(1);
            staticEffect.Parameters["staticPositions"].SetValue(new float[1] { 0.5f });
            staticEffect.Parameters["staticThicknesses"].SetValue(new float[1] { 1 });
            staticEffect.Parameters["whiteness"].SetValue(STATIC_WHITENESS);
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise,
            staticEffect, Matrix.Identity);
            spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle(0, 0, staticRenderTarget.Width, staticRenderTarget.Height),
                null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatchHUD.End();
        }

        public void DrawMenu()
        {
            Vector2 texSize = new Vector2(finalRenderTarget.Width, finalRenderTarget.Height);

            Vector2 position = new Vector2(MENU_BOTTOM_OPTION_POSITION.X, MENU_BOTTOM_OPTION_POSITION.Y - (MENU_POSITION_VERTICAL_STEP * (options.Count + 1)));
            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, options.Title, position * texSize, MENU_COLOR_TITLE, 0f, Vector2.Zero, MENU_OPTIONS_TITLE_SCALE, SpriteEffects.None, 0f);
            position.Y += MENU_POSITION_VERTICAL_STEP;
            for (int i = 0; i < options.Count; i++)
            {
                float arrowDistanceOffset = ARROW_DISTANCE_MIN;
                Color menuItemColor;
                if (i == currentMenuIndex)
                {
                    menuItemColor = MENU_COLOR_SELECTED;
                    arrowDistanceOffset = (options.IsEnabled(i)) ? arrowDistance : ARROW_DISTANCE_MIN;
                }
                else
                    menuItemColor = (options.IsEnabled(i)) ? MENU_COLOR_UNSELECTED : MENU_COLOR_UNAVAILABLE;
                if (MenuOptions.IsLeftRightAction(options[i]))
                {
                    string optionsString = options[i].Key.Substring(1, options[i].Key.Length - 2);
                    float optionsStringLength = RetroGame.FONT_PIXEL_LARGE.MeasureString(optionsString).X / finalRenderTarget.Width; //relative
                    spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, optionsString, position * texSize, menuItemColor, 0f, Vector2.Zero, MENU_OPTIONS_TEXT_SCALE, SpriteEffects.None, 0f);
                    spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, "<", new Vector2(position.X - arrowDistanceOffset, position.Y) * texSize, menuItemColor, 0f, Vector2.Zero, MENU_OPTIONS_TEXT_SCALE, SpriteEffects.None, 0f);
                    float arrowLength = (RetroGame.FONT_PIXEL_LARGE.MeasureString(">").X / finalRenderTarget.Width); //relative
                    spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, ">", new Vector2(position.X - arrowLength + optionsStringLength + arrowDistanceOffset, position.Y) * texSize, menuItemColor, 0f, Vector2.Zero, MENU_OPTIONS_TEXT_SCALE, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, options[i].Key, position * texSize, menuItemColor, 0f, Vector2.Zero, MENU_OPTIONS_TEXT_SCALE, SpriteEffects.None, 0f);
                }
                position.Y += MENU_POSITION_VERTICAL_STEP;
            }

            float letterSizeOffset = (RetroGame.FONT_PIXEL_LARGE.MeasureString("M").Y / folderRenderTarget.Height) / 2;
            switch (settingsMode)
            {
                case SettingsMode.None:
                    break;
                case SettingsMode.Bindings:
                    position.Y = MENU_BOTTOM_OPTION_POSITION.Y - (MENU_POSITION_VERTICAL_STEP * (options.Count - 1)) + letterSizeOffset;
                    List<Bindings.Binding> bindingsToDraw = usesCustomBindings ? customBindingsToModify : Bindings.DEFAULT_BINDINGS[currentPlayerBindingsMenu];
                    for (int i = 0; i < bindingsToDraw.Count; i++)
                    {
                        position.X = KEY_TEXT_XPOS;
                        spriteBatchHUD.DrawString(RetroGame.FONT_DEBUG, bindingsToDraw[i].Key.ToString(), position * texSize, KEY_BINDINGS_COLOR, 0, RetroGame.FONT_DEBUG.MeasureString(bindingsToDraw[i].Key.ToString()) / 2, KEY_TEXT_SCALE, SpriteEffects.None, 0);
                        position.X = KEY_BINDINGS_XPOS;
                        string keyString = Bindings.GetHUDIconCharacter(bindingsToDraw[i].Key);
                        Vector2 dims = RetroGame.FONT_HUD_KEYS.MeasureString(keyString);
                        spriteBatchHUD.DrawString(RetroGame.FONT_HUD_KEYS, keyString, position * texSize, KEY_BINDINGS_COLOR, 0, dims / 2, KEY_BINDINGS_SCALE, SpriteEffects.None, 0);
                        position.X = XBOX_BINDINGS_XPOS;
                        if(new[] {Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight}.Contains(bindingsToDraw[i].Button))
                            keyString = "&";
                        else
                            keyString = Bindings.GetHUDIconCharacter(bindingsToDraw[i].Button);
                        dims = RetroGame.FONT_HUD_XBOX.MeasureString(keyString);
                        spriteBatchHUD.DrawString(RetroGame.FONT_HUD_XBOX, keyString, position * texSize, Color.White, 0, dims / 2, XBOX_BINDINGS_SCALE, SpriteEffects.None, 0);
                        position.Y += MENU_POSITION_VERTICAL_STEP;
                    }
                    spriteBatchHUD.DrawString(RetroGame.FONT_DEBUG, bindingsMenuInstructions, BINDINGS_INSTRUCTIONS_POS * texSize, bindingsMenuInstructionsColor, 0, Vector2.Zero, BINDINGS_INSTRUCTIONS_SCALE, SpriteEffects.None, 0);
                    break;
                case SettingsMode.Sound:
                    const float OUTER_SHRINKAGE_Y = 0.10f;
                    const float INNER_SHRINKAGE_X = 0.025f;
                    const float INNER_SHRINKAGE_Y = 0.25f;
                    position = new Vector2(VOLUME_BAR_XPOS, MENU_BOTTOM_OPTION_POSITION.Y - (MENU_POSITION_VERTICAL_STEP * (options.Count)) + letterSizeOffset);
                    Vector2 outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    Vector2 innerBarPos = new Vector2(position.X + (VOLUME_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    Vector2 iconPos = new Vector2(VOLUME_ICON_XPOS, position.Y) * texSize;
                    Vector2 outerBarSize = new Vector2(VOLUME_BAR_WIDTH, MENU_POSITION_VERTICAL_STEP * (1 - OUTER_SHRINKAGE_Y * 2)) * texSize;
                    Vector2 innerBarSize = new Vector2(VOLUME_BAR_WIDTH * (1 - INNER_SHRINKAGE_X * 2), MENU_POSITION_VERTICAL_STEP * (1 - INNER_SHRINKAGE_Y * 2)) * texSize;
                    Vector2 iconSize = outerBarSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, VOLUME_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * SoundManager.MasterVolume), (int)innerBarSize.Y), null, VOLUME_BAR_COLOR_INNER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw((SoundManager.MasterVolume > 0) ? soundLoudTex : soundMuteTex, new Rectangle((int)iconPos.X, (int)iconPos.Y, (int)outerBarSize.Y, (int)outerBarSize.Y), null, Color.White, 0, new Vector2(soundLoudTex.Width / 2f, soundLoudTex.Height / 2f), SpriteEffects.None, 0); //scale it to a square the heigh of the outer bar
                    position.Y += MENU_POSITION_VERTICAL_STEP;
                    outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    innerBarPos = new Vector2(position.X + (VOLUME_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    iconPos = new Vector2(VOLUME_ICON_XPOS, position.Y) * texSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, VOLUME_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * SoundManager.MusicMasterVolume), (int)innerBarSize.Y), null, VOLUME_BAR_COLOR_INNER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw((SoundManager.MusicMasterVolume > 0) ? soundLoudTex : soundMuteTex, new Rectangle((int)iconPos.X, (int)iconPos.Y, (int)outerBarSize.Y, (int)outerBarSize.Y), null, Color.White, 0, new Vector2(soundLoudTex.Width / 2f, soundLoudTex.Height / 2f), SpriteEffects.None, 0);
                    position.Y += MENU_POSITION_VERTICAL_STEP;
                    outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    innerBarPos = new Vector2(position.X + (VOLUME_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    iconPos = new Vector2(VOLUME_ICON_XPOS, position.Y) * texSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, VOLUME_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * SoundManager.SoundMasterVolume), (int)innerBarSize.Y), null, VOLUME_BAR_COLOR_INNER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    spriteBatchHUD.Draw((SoundManager.SoundMasterVolume > 0) ? soundLoudTex : soundMuteTex, new Rectangle((int)iconPos.X, (int)iconPos.Y, (int)outerBarSize.Y, (int)outerBarSize.Y), null, Color.White, 0, new Vector2(soundLoudTex.Width / 2f, soundLoudTex.Height / 2f), SpriteEffects.None, 0);
                    break;
                case SettingsMode.Menu:
                    break;
                case SettingsMode.ScreenSize:
                    break;
            }
        }

        public override void Dispose()
        {
            tab1RenderTarget.Dispose();
            tab2RenderTarget.Dispose();
            photoRenderTarget.Dispose();
            paperRenderTarget.Dispose();
            folderRenderTarget.Dispose();
            staticRenderTarget.Dispose();
            finalRenderTarget.Dispose();
            spriteBatchHUD.Dispose();

            soundRenderTarget.Dispose();
        }
    }
}
