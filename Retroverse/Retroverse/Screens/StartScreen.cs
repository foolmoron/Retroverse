using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class StartScreen : MenuScreen
    {
        public const float STATIC_TRANSITION_TIME = 1f;

        public int activePlayerFolder;
        public const float FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN = 1.5f;
        public const float FOLDER_SCREEN_POSITION_RELATIVE_SHOWN = 0.5f;
        public const float FOLDER_ROTATION_HIDDEN = 0f;
        public const float FOLDER_ROTATION_SHOWN = 0.08f;
        public const float FOLDER_SCREEN_VELOCITY_RELATIVE = 1f;
        public float folderPositionRelative = FOLDER_SCREEN_POSITION_RELATIVE_SHOWN;
        public Vector2 folderPosition = new Vector2(-RetroGame.screenSize.X, -RetroGame.screenSize.Y); // prevent first frame from showing in a bad position on screen
        public float folderRotation;

        public static readonly Vector2 PHOTO_POSITION_RELATIVE = new Vector2(0.12f, 0.115f);
        public static readonly Vector2 PAPER_RIGHT_POSITION_RELATIVE = new Vector2(0.52f, 0.14f);
        public static readonly Vector2 PAPER_LEFT_POSITION_RELATIVE = new Vector2(0.07f, 0.17f);
        public static readonly Vector2 TAB_POSITION_RELATIVE = new Vector2(0.65f, 0.0f);
        public static readonly Vector2 LOGO_POSITION_RELATIVE = new Vector2(0.07f, 0.10f);

        public static readonly Vector2 PHOTO_HERO_POSITION_RELATIVE = new Vector2(0.15f, 0.10f);
        public static readonly Vector2 PHOTO_HERO_SIZE_RELATIVE = new Vector2(0.70f, 0.70f);
        public static readonly Vector2 PHOTO_NAME_POSITION_RELATIVE = new Vector2(0.5f, 0.87f);
        public static readonly float PHOTO_NAME_MAXWIDTH_RELATIVE = 0.8f;
        public static readonly Color PHOTO_NAME_COLOR = Color.Black;

        public readonly MenuOptions startOptions;

        //WASD instructions
        public bool enableDrawWASDInstructions = false;
        public static readonly Vector2 WASD_NAVIGATE_POSITION = new Vector2(0.15f, 0.35f);
        public static readonly Vector2 WASD_PLUS_POSITION = new Vector2(0.2575f, 0.345f);
        public static readonly Vector2 WASD_SELECT_POSITION = new Vector2(0.2855f, 0.355f);
        public static float WASD_SCALE = 0.5f;

        //setup
        public MenuOptions setupOptions;
        public static int currentSetupIndex;
        public const int NUM_COLOR_STEPS = 8;
        public const byte COLOR_STEP = (byte)(255f / NUM_COLOR_STEPS);
        public const byte COLOR_MIN = 32;
        public const float SETUP_XPOS = 0.57f;
        public const float SETUP_YPOS = 0.5317383f;
        public const float COLOR_BAR_WIDTH = 0.25f;
        public static readonly Color COLOR_BAR_COLOR_OUTER = Color.Black;

        public const string NAME_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static bool setupTypingName = false;
        public static string setupTypedName = "";
        public static readonly string setupTypingInstructions = "Type a name of up to " + Names.CHAR_LIMIT + "\ncharacters and press\n[ENTER] to save.\nPress [ESC] to cancel.";
        public static readonly Vector2 SETUP_TYPING_INSTRUCTIONS_POS = new Vector2(0.56f, 0.35f);

        //highscores
        public static Vector2 HIGHSCORES_POS = new Vector2(0.55f, 0.3f);
        public static float HIGHSCORES_SCALE = 1.0f;
        
        //continue
        public readonly MenuOptions continueOptions;
        private List<SaveGame> continueSaves;
        public const int LOADDELETE = 5;
        public const int CONTINUE_SAVES_TO_DISPLAY = 7;
        public const int CONTINUE_SAVES_CENTER = CONTINUE_SAVES_TO_DISPLAY / 2;
        public float CONTINUE_SAVES_CENTER_YPOS = 0.46f;
        public const float CONTINUE_SAVES_NAME_XPOS = 0.552f;
        public const float CONTINUE_SAVES_CELL_XPOS = 0.77f;
        public const float CONTINUE_SAVES_TIME_XPOS = 0.552f;
        public const float CONTINUE_SAVES_COOP_XPOS = 0.83f;
        public static readonly Color CONTINUE_SAVES_TEXT_COLOR = Color.Black;
        public static readonly Color CONTINUE_SAVES_SELECTED_COLOR = Color.Cyan;
        public const float CONTINUE_SAVES_COLOR_SPEED = 1f;
        public static float continueSavesColorInterp = 0;
        public static int continueSavesColorInterpModifier = 1;
        public static Color continueSavesSelectedColor = CONTINUE_SAVES_TEXT_COLOR;
        public const float CONTINUE_SAVES_TEXT_SCALE = 1f;
        private int continueSaveSelectedIndex;
        private enum ContinueSortMode { DateDesc, DateAsc, NameDesc, NameAsc, ProgressDesc, ProgressAsc };
        private ContinueSortMode continueSortMode;
        private static readonly Dictionary<ContinueSortMode, string> CONTINUE_SORT_TO_STRING = new Dictionary<ContinueSortMode, string>()
        {
            {ContinueSortMode.DateDesc, "Date-"},
            {ContinueSortMode.DateAsc, "Date+"},
            {ContinueSortMode.NameDesc, "Name-"},
            {ContinueSortMode.NameAsc, "Name+"},
            {ContinueSortMode.ProgressDesc, "Cell-"},
            {ContinueSortMode.ProgressAsc, "Cell+"},
        };
        public const string NO_SAVES_MESSAGE = "No recorded\nprisoner\nfootage found.";
        public const float NO_SAVES_SCALE = 0.85f;

        //credits
        public readonly MenuOptions creditsOptions;
        public const float CREDITS_LEFT_ALIGN = 0.555f;
        public const float CREDITS_TOP_YPOS = 0.4f;
        public float CREDITS_LINE_SPACING = 0.0275f;
        public float CREDITS_LINE_SCALE = 1.15f;
        public static readonly Color CREDITS_TITLE_COLOR = Color.Black;
        public static readonly Color CREDITS_HIGHLIGHT_COLOR = Color.DarkCyan;

        public static int currentCreditsPage;
        public const int NUM_CREDITS_PAGES = 2;
        #region Credits Lists
        public static readonly string[][] CREDITS_LINES = new string[][]
        {
            new string[]
            {
                "Director",
                "Design",
                "Programming",
                "Momin Khan",
                "Graphics",
                "Zane Laughlin",
                "Audio",
                "Nick Lytle",
                "Level Design",
                "Playtesting",
                "Programming",
                "Rachel Brown",
                "Martin Kellogg",
                "John Murdock",
                "Thomas Sparks",
            },
            new string[]
            {
                "Source Code",
                "github.com/foolmoron",
                "Tools",
                "Microsoft VS 2010",
                "Microsoft XNA 4.0",
                "JetBrains ReSharper",
                "Victor's Pixel Font",
                "Sergeant Koopa",
                "KEYmode font",
                "Flop Design",
                "XNA Button Pack 3",
                "Jeff Jenkins (@Sinnix)",
                "Code snippets",
                "StackOverflow",
                "Wikipedia",
            },
        };
        public static readonly bool[][] CREDITS_HIGHLIGHTS = new bool[][]
        {
            new bool[]
            {
                false,
                false,
                false,
                true,
                false,
                true,
                false,
                true,
                false,
                false,
                false,
                true,
                true,
                true,
                true,
            },
            new bool[]
            {
                false,
                true,
                false,
                true,
                true,
                true,
                false,
                true,
                false,
                true,
                false,
                true,
                false,
                true,
                true,
            },
        };
        #endregion

        public StartScreen(Bindings activeBindings)
            : base(activeBindings)
        {
            TransitionOutEnabled = false;
            switch (activeBindings.PlayerIndex)
            {
                case PlayerIndex.One:
                    activePlayerFolder = Player.One;
                    break;
                case PlayerIndex.Two:
                    activePlayerFolder = Player.Two;
                    break;
            }
           
            startOptions = new MenuOptions("Start",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"Solo escape", delegate { settingsMode = SettingsMode.Setup; InitializeSetupOptions(1); SetMenuOptions(setupOptions); }},
                    {"Co-op escape", delegate { settingsMode = SettingsMode.Setup; InitializeSetupOptions(2); SetMenuOptions(setupOptions); }},
                    {"Continue", delegate
                        {
                            settingsMode = SettingsMode.Continue; 
                            continueSortMode = ContinueSortMode.DateDesc; 
                            SetMenuOptions(continueOptions);
                            continueSaveSelectedIndex = 0;
                            updateContinueMenuOptions();
                        }},
                    {"Settings", delegate { settingsMode = SettingsMode.Menu; SetMenuOptions(GetSettingsOptions()); }},
                    {"Credits", delegate { settingsMode = SettingsMode.Credits; SetMenuOptions(creditsOptions); }},
                    {"Exit game", delegate { DisplayConfirmationDialog("Exit?", delegate { RetroGame.game.Exit(); }); }},
                }
                , (Action<MenuOptionAction>)null);
            Action<MenuOptionAction> loadDeleteAction = null;
            loadDeleteAction = delegate(MenuOptionAction action)
                {
                    switch (action)
                    {
                        case MenuOptionAction.Click:
                            if (options[LOADDELETE].Key == "<Load>")
                            {
                                RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.ToStatic, STATIC_TRANSITION_TIME,
                                    delegate
                                    {
                                        RetroGame.PopScreen(true);
                                        RetroGame.NUM_PLAYERS = continueSaves[continueSaveSelectedIndex].numPlayers;
                                        SaveGame loadedGame = RetroGame.Load(continueSaves[continueSaveSelectedIndex].filename);
                                        if (loadedGame != null)
                                        {
                                            Type[] powerupTypes = new Type[loadedGame.storePowerupTypeNames.Length];
                                            for (int i = 0; i < powerupTypes.Length; i++)
                                                powerupTypes[i] = Type.GetType(loadedGame.storePowerupTypeNames[i]);
                                            RetroGame.AddScreen(new StoreScreen(powerupTypes), true);
                                        }
                                    }));
                            }
                            else if (options[LOADDELETE].Key == "<Delete>")
                            {
                                DisplayConfirmationDialog("Delete?", delegate
                                    {
                                        Saves.DeleteSave(continueSaves[continueSaveSelectedIndex].filename);
                                        SetMenuOptions(continueOptions);
                                        updateContinueMenuOptions();
                                        if (continueSaveSelectedIndex > (continueSaves.Count - 1))
                                            continueSaveSelectedIndex = continueSaves.Count - 1;
                                    });
                            }
                            break;
                        case MenuOptionAction.Left:
                        case MenuOptionAction.Right:
                            if (options[LOADDELETE].Key == "<Load>")
                                options[LOADDELETE] = new KeyValuePair<string, Action<MenuOptionAction>>("<Delete>", loadDeleteAction);
                            else if (options[LOADDELETE].Key == "<Delete>")
                                options[LOADDELETE] = new KeyValuePair<string, Action<MenuOptionAction>>("<Load>", loadDeleteAction);
                            break;
                    }
                };
            continueOptions = new MenuOptions("Continue ",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"<Sort:" + CONTINUE_SORT_TO_STRING[ContinueSortMode.DateDesc] + ">", delegate (MenuOptionAction action)
                        {
	                        int sortIndex = (int)continueSortMode;
                            switch (action)
	                        {
                                case MenuOptionAction.Click:
                                case MenuOptionAction.Right:
	                                if(sortIndex < (Enum.GetValues(typeof(ContinueSortMode)).Length - 1))
	                                    sortIndex++;
	                                else
	                                    sortIndex = 0;
                                    break;
                                case MenuOptionAction.Left:
                                    if (sortIndex > 0)
                                        sortIndex--;
                                    else
                                        sortIndex = Enum.GetValues(typeof(ContinueSortMode)).Length - 1;
                                    break;
	                        }
                            continueSortMode = (ContinueSortMode)Enum.GetValues(typeof(ContinueSortMode)).GetValue(sortIndex);
                            options[0] = new KeyValuePair<string, Action<MenuOptionAction>>("<Sort:" + CONTINUE_SORT_TO_STRING[continueSortMode] + ">", options[0].Value);
                            sortSaves();
                        }},
                    {@"/\/\/\", delegate { continueSaveSelectedIndex = 0; }},
                    {@"  /\  ", delegate { if(continueSaveSelectedIndex > 0) continueSaveSelectedIndex--; }},
                    {@"  \/  ", delegate { if(continueSaveSelectedIndex < (continueSaves.Count - 1)) continueSaveSelectedIndex++; }},
                    {@"\/\/\/", delegate { continueSaveSelectedIndex = continueSaves.Count - 1; }},
                    {"<Load>", loadDeleteAction},
                    {"Back", delegate{ settingsMode = SettingsMode.None; SetMenuOptions(startOptions); }},
                }
                , "Back");
            creditsOptions = new MenuOptions("Credits",
                new Dictionary<string, Action<MenuOptionAction>>()
                {
                    {"Developers", null },
                    {"Extra", null },
                    {"Back", delegate { settingsMode = SettingsMode.None; SetMenuOptions(startOptions); }},
                }
                , "Back");

            SetMenuOptions(startOptions);
        }

        public void InitializeSetupOptions(int numPlayers)
        {
            for(int i = 0; i < RetroGame.MAX_PLAYERS; i++)
            {
                Hero.NEW_HERO_IDS[i] = Prisoner.getRandomPrisonerID();
                Hero.NEW_HERO_NAMES[i] = Names.getRandomName();
                Hero.NEW_HERO_COLORS[i] = Hero.NEW_HERO_COLORS[i].Randomize(COLOR_MIN, 255);
            }
            setupTypingName = false;
            setupTypedName = "";

            Action soloTransitionAction = delegate { RetroGame.PopScreen(true); RetroGame.NUM_PLAYERS = 1; RetroGame.Load(Saves.NEW_GAME); RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.FromStatic, STATIC_TRANSITION_TIME, null), true); };
            Action coopTransitionAction = delegate { RetroGame.PopScreen(true); RetroGame.NUM_PLAYERS = 2; RetroGame.Load(Saves.NEW_GAME); RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.FromStatic, STATIC_TRANSITION_TIME, null), true); };

            currentSetupIndex = Player.One;
            KeyValuePair<string, Action<MenuOptionAction>> startOption = new KeyValuePair<string, Action<MenuOptionAction>>
                ("Start", (numPlayers == 1) ? new Action<MenuOptionAction>(delegate { RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.ToStatic, STATIC_TRANSITION_TIME, soloTransitionAction), true); })
                                            : new Action<MenuOptionAction>(delegate { RetroGame.AddScreen(new StaticTransitionScreen(TransitionMode.ToStatic, STATIC_TRANSITION_TIME, coopTransitionAction), true); }));
            KeyValuePair<string, Action<MenuOptionAction>> nextOption = new KeyValuePair<string, Action<MenuOptionAction>>
                ("Next", delegate { currentSetupIndex++; setupOptions.Title = "Setup P" + (currentSetupIndex + 1); setupOptions[6] = startOption; });
            bool useStartOption = numPlayers == 1;
            setupOptions = new MenuOptions("Setup P1",
                    new Dictionary<string, Action<MenuOptionAction>>()
                    {
                        {"Randomize", delegate
                            {
                                Hero.NEW_HERO_IDS[currentSetupIndex] = Prisoner.getRandomPrisonerID();
                                Hero.NEW_HERO_NAMES[currentSetupIndex] = Names.getRandomName();
                                Hero.NEW_HERO_COLORS[currentSetupIndex] = Hero.NEW_HERO_COLORS[currentSetupIndex].Randomize(COLOR_MIN, 255);
                            }},
                        {"ID", delegate { Hero.NEW_HERO_IDS[currentSetupIndex] = Prisoner.getRandomPrisonerID(); }},
                        {"Name", setNameDelegate},
                        {"<R>", delegate(MenuOptionAction action)
                            {
                                switch (action)
                                {
                                    case MenuOptionAction.Click:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].R = (byte)(RetroGame.rand.Next(255 - COLOR_MIN) + COLOR_MIN);
                                        break;
                                    case MenuOptionAction.Left:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].R = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].R - COLOR_STEP, COLOR_MIN, 255);
                                        break;
                                    case MenuOptionAction.Right:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].R = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].R + COLOR_STEP, COLOR_MIN, 255);
                                        break;
	                            }
                            }},
                        {"<G>", delegate(MenuOptionAction action)
                            {
                                switch (action)
                                {
                                    case MenuOptionAction.Click:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].G = (byte)(RetroGame.rand.Next(255 - COLOR_MIN) + COLOR_MIN);
                                        break;
                                    case MenuOptionAction.Left:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].G = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].G - COLOR_STEP, COLOR_MIN, 255);
                                        break;
                                    case MenuOptionAction.Right:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].G = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].G + COLOR_STEP, COLOR_MIN, 255);
                                        break;
	                            }
                            }},
                        {"<B>", delegate(MenuOptionAction action)
                            {
                                switch (action)
                                {
                                    case MenuOptionAction.Click:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].B = (byte)(RetroGame.rand.Next(255 - COLOR_MIN) + COLOR_MIN);
                                        break;
                                    case MenuOptionAction.Left:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].B = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].B - COLOR_STEP, COLOR_MIN, 255);
                                        break;
                                    case MenuOptionAction.Right:
                                        Hero.NEW_HERO_COLORS[currentSetupIndex].B = (byte)MathHelper.Clamp(Hero.NEW_HERO_COLORS[currentSetupIndex].B + COLOR_STEP, COLOR_MIN, 255);
                                        break;
	                            }
                            }},
                        {useStartOption ? startOption.Key : nextOption.Key, useStartOption ? startOption.Value : nextOption.Value},
                        {"Back", delegate
                            {
                               if (currentSetupIndex == 0) 
                                   DisplayConfirmationDialog("Back?", delegate { settingsMode = SettingsMode.None; SetMenuOptions(startOptions); }); 
                               else
                               {
                                   currentSetupIndex--;
                                   setupOptions.Title = "Setup P" + (currentSetupIndex + 1); 
                                   setupOptions[6] = nextOption;
                               } 
                            }},
                    }
                , "Back");
        }

        public void setNameDelegate(MenuOptionAction menuOptionAction)
        {
            SoundManager.PlaySoundOnce("ButtonForward");
            setupTypingName = true;
            setupTypedName = "";
            Action<Keys?, Buttons?> nameTypingDelegate = null;
            nameTypingDelegate = delegate(Keys? pressedKey, Buttons? pressedButton)
            {
                if (pressedKey != null)
                {
                    if (pressedKey.Value == Keys.Escape || pressedKey.Value == Keys.Enter)
                    {
                        setupTypingName = false;
                        if (pressedKey.Value != Keys.Escape)
                        {
                            if (setupTypedName.Length > 0)
                            {
                                Hero.NEW_HERO_NAMES[currentSetupIndex] = setupTypedName;
                                SoundManager.PlaySoundOnce("ButtonForward");
                            }
                            else
                                SoundManager.PlaySoundOnce("ButtonFailure");
                        }
                        else
                            SoundManager.PlaySoundOnce("ButtonBack");
                        return;
                    }
                    if (NAME_ALPHABET.Contains(pressedKey.Value.ToString()))
                        setupTypedName += (setupTypedName.Length == 0) ? pressedKey.Value.ToString().ToUpper() : pressedKey.Value.ToString().ToLower();
                    if (setupTypedName.Length >= Names.CHAR_LIMIT)
                    {
                        setupTypingName = false;
                        if (setupTypedName.Length > 0)
                        {
                            Hero.NEW_HERO_NAMES[currentSetupIndex] = setupTypedName;
                            SoundManager.PlaySoundOnce("ButtonForward");
                        }
                        else
                            SoundManager.PlaySoundOnce("ButtonFailure");
                        return;
                    }
                    if (setupTypedName.Length > 0 && (pressedKey.Value == Keys.Back || pressedKey.Value == Keys.Delete))
                    {
                        setupTypedName = setupTypedName.Substring(0, setupTypedName.Length - 1);
                    }
                }
                currentBindings.onNextInputAction = nameTypingDelegate; //keep capturing input
            };
            currentBindings.onNextInputAction = nameTypingDelegate;
        }

        public override void OnScreenSizeChanged()
        {
            base.OnScreenSizeChanged();
            folderPosition = new Vector2(0.5f, folderPositionRelative) * RetroGame.screenSize;
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            base.OnInputAction(action, pressedThisFrame);
            if (RetroGame.NUM_PLAYERS > 1 && pressedThisFrame)
            {
                switch (action)
                {
                    case InputAction.Left:
                        activePlayerFolder = (activePlayerFolder + 1) % RetroGame.NUM_PLAYERS;
                        SoundManager.PlaySoundOnce("ButtonForward");
                        break;
                    case InputAction.Right:
                        activePlayerFolder--;
                        if (activePlayerFolder < 0)
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
            switch (mode)
            {
                case MenuScreenMode.TransitioningIn:
                    {
                        folderPositionRelative = MathHelper.Clamp(folderPositionRelative -= FOLDER_SCREEN_VELOCITY_RELATIVE * seconds, FOLDER_SCREEN_POSITION_RELATIVE_SHOWN, FOLDER_SCREEN_POSITION_RELATIVE_HIDDEN);
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
                        }
                    }
                    break;
            }
            if (settingsMode == SettingsMode.Continue)
            {
                continueSavesColorInterp += seconds * continueSavesColorInterpModifier * CONTINUE_SAVES_COLOR_SPEED;
                if (continueSavesColorInterp > 1 || continueSavesColorInterp < 0)
                {
                    continueSavesColorInterpModifier *= -1;
                    continueSavesColorInterp = MathHelper.Clamp(continueSavesColorInterp, 0, 1);
                }
                continueSavesSelectedColor = Color.Lerp(CONTINUE_SAVES_TEXT_COLOR, CONTINUE_SAVES_SELECTED_COLOR, continueSavesColorInterp);
            }
        }

        public void updateContinueMenuOptions()
        {
            continueSaves = Saves.GetAllSaves();
            sortSaves();
            options.SetEnabled(continueSaves.Count > 0, 0, LOADDELETE);
        }

        public void sortSaves()
        {
            if (continueSaves == null || continueSaves.Count == 0)
                return;
            Func<SaveGame, SaveGame, int> nameComparison = (a, b) => a.heroStates[0].name.CompareTo(b.heroStates[0].name);
            Func<SaveGame, SaveGame, int> dateComparison = delegate(SaveGame a, SaveGame b)
                {
                    int ret = 0;
                    long diff = a.time.Ticks - b.time.Ticks;
                    if(diff > 0)
                        ret = 1;
                    else if(diff < 0)
                        ret = -1;
                    else
                        ret = nameComparison(a, b);
                    return ret;
                };
            Func<SaveGame, SaveGame, int> progressComparison = (a, b) => (a.levelX - b.levelX) == 0 ? nameComparison(a, b) : a.levelX - b.levelX;
            switch (continueSortMode)
            {
                case ContinueSortMode.DateDesc:
                    continueSaves.Sort((a, b) => -1 * dateComparison(a, b));
                    break;
                case ContinueSortMode.DateAsc:
                    continueSaves.Sort((a, b) => dateComparison(a, b));
                    break;
                case ContinueSortMode.NameDesc:
                    continueSaves.Sort((a, b) => nameComparison(a, b));
                    break;
                case ContinueSortMode.NameAsc:
                    continueSaves.Sort((a, b) => -1 * dateComparison(a, b));
                    break;
                case ContinueSortMode.ProgressDesc:
                    continueSaves.Sort((a, b) => -1 * progressComparison(a, b));
                    break;
                case ContinueSortMode.ProgressAsc:
                    continueSaves.Sort((a, b) => progressComparison(a, b));
                    break;
            }
        }

        public override void PreDraw(GameTime gameTime)
        {
            Hero hero = RetroGame.getHeroes()[activePlayerFolder];
            DrawTab(Player.None);
            DrawPhoto();
            if (settingsMode == SettingsMode.Setup)
            {
                spriteBatchHUD.Begin();
                Point pos = new Point((int)(PHOTO_HERO_POSITION_RELATIVE.X * photoRenderTarget.Width), (int)(PHOTO_HERO_POSITION_RELATIVE.Y * photoRenderTarget.Height));
                Point size = new Point((int)(PHOTO_HERO_SIZE_RELATIVE.X * photoRenderTarget.Width), (int)(PHOTO_HERO_SIZE_RELATIVE.Y * photoRenderTarget.Height));
                spriteBatchHUD.Draw(TextureManager.Get("hero"), new Rectangle(pos.X, pos.Y, size.X, size.Y), null, Hero.NEW_HERO_COLORS[currentSetupIndex], 0, Vector2.Zero, SpriteEffects.None, 0);
                string nameString = "#" + Hero.NEW_HERO_IDS[currentSetupIndex].ToString("0000") + " " + Hero.NEW_HERO_NAMES[currentSetupIndex];
                Vector2 nameDims = RetroGame.FONT_PIXEL_LARGE.MeasureString(nameString);
                float nameScale = (PHOTO_NAME_MAXWIDTH_RELATIVE * photoRenderTarget.Width) / nameDims.X;
                spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, nameString, new Vector2(PHOTO_NAME_POSITION_RELATIVE.X * photoRenderTarget.Width, PHOTO_NAME_POSITION_RELATIVE.Y * photoRenderTarget.Height), PHOTO_NAME_COLOR, 0, nameDims / 2, nameScale, SpriteEffects.None, 0);
                spriteBatchHUD.End();
            }
            DrawPaper();
            DrawFolder();
            DrawStatic();

            GraphicsDevice.SetRenderTarget(finalRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Vector2 texSize = new Vector2(finalRenderTarget.Width, finalRenderTarget.Height);
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatchHUD.Draw(folderRenderTarget, Vector2.Zero, Color.White);
            spriteBatchHUD.Draw(paperRenderTarget, PAPER_RIGHT_POSITION_RELATIVE * texSize, Color.White);
            spriteBatchHUD.Draw(tab1RenderTarget, TAB_POSITION_RELATIVE * texSize, Color.White);
            if (settingsMode != SettingsMode.Bindings)
            {
                spriteBatchHUD.Draw(photoRenderTarget, PHOTO_POSITION_RELATIVE * texSize, Color.White);
                if (settingsMode != SettingsMode.Setup)
                {
                    spriteBatchHUD.Draw(TextureManager.Get("retroversetitle5"), LOGO_POSITION_RELATIVE * texSize, Color.White);
                }
            }
            DrawMenu();
            switch (settingsMode)
            {
                case SettingsMode.None:
                case SettingsMode.Menu:
                    if (enableDrawWASDInstructions)
                    {
                        spriteBatchHUD.DrawString(RetroGame.FONT_HUD_KEYS, "WASD", WASD_NAVIGATE_POSITION * texSize, Color.Black, 0, Vector2.Zero, WASD_SCALE, SpriteEffects.None, 0);
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, "+", WASD_PLUS_POSITION * texSize, Color.Black, 0, Vector2.Zero, WASD_SCALE, SpriteEffects.None, 0);
                        spriteBatchHUD.DrawString(RetroGame.FONT_HUD_KEYS, "_", WASD_SELECT_POSITION * texSize, Color.Black, 0, Vector2.Zero, WASD_SCALE, SpriteEffects.None, 0);
                    }
                    Highscores.Draw(Highscores.DrawMode.Both, spriteBatchHUD, HIGHSCORES_POS * texSize, HIGHSCORES_SCALE);
                    break;
                case SettingsMode.Continue: 
                    Vector2 position;
                    position.Y = CONTINUE_SAVES_CENTER_YPOS;
                    int saveCount = continueSaves.Count;
                    if (saveCount > 0)
                    {
                        int startSaveIndex = continueSaveSelectedIndex - CONTINUE_SAVES_CENTER;
                        for (int i = 0; i < CONTINUE_SAVES_TO_DISPLAY; i++)
                        {
                            int index = i + startSaveIndex;
                            if (index < 0 || index > (saveCount - 1))
                            {
                                position.Y += MENU_POSITION_VERTICAL_STEP;
                                continue;
                            }

                            SaveGame save = continueSaves[index];
                            int distanceFromCenter = Math.Abs(i - CONTINUE_SAVES_CENTER);
                            const float DISTANCE_FADE = 150;
                            byte alpha = (distanceFromCenter != 0) ? (byte)((255 - DISTANCE_FADE) + (DISTANCE_FADE * (CONTINUE_SAVES_CENTER - distanceFromCenter) / CONTINUE_SAVES_CENTER)) : (byte)255;
                            Color color = (distanceFromCenter != 0) ? CONTINUE_SAVES_TEXT_COLOR.withAlpha(alpha) : continueSavesSelectedColor;
                            position.X = CONTINUE_SAVES_NAME_XPOS;
                            string nameString = "#" + save.heroStates[0].id.ToString("0000") + " " + save.heroStates[0].name;
                            Vector2 nameDims = RetroGame.FONT_PIXEL_SMALL.MeasureString(nameString);
                            float nameModifiedScale = ((CONTINUE_SAVES_CELL_XPOS - position.X) * texSize.X) / nameDims.X;
                            float nameScale = Math.Min(CONTINUE_SAVES_TEXT_SCALE, nameModifiedScale);
                            Vector2 namePos = position * texSize;
                            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, nameString, new Vector2(namePos.X, namePos.Y + nameDims.Y / 2), color, 0, new Vector2(0, nameDims.Y / 2), nameScale, SpriteEffects.None, 0);
                            position.X = CONTINUE_SAVES_CELL_XPOS;
                            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, Level.GetCellName(save.levelX, save.levelY, save.cellOffset1, save.cellOffset2), position * texSize, color, 0, Vector2.Zero, CONTINUE_SAVES_TEXT_SCALE, SpriteEffects.None, 0);
                            position.X = CONTINUE_SAVES_COOP_XPOS;
                            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, (save.numPlayers == 1) ? "SOLO" : "COOP", position * texSize, color, 0, Vector2.Zero, CONTINUE_SAVES_TEXT_SCALE, SpriteEffects.None, 0);
                            position.Y += MENU_POSITION_VERTICAL_STEP / 2;
                            position.X = CONTINUE_SAVES_TIME_XPOS;
                            spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, save.time.ToString(), position * texSize, color, 0, Vector2.Zero, CONTINUE_SAVES_TEXT_SCALE, SpriteEffects.None, 0);
                            position.Y += MENU_POSITION_VERTICAL_STEP / 2;
                        }
                    }
                    else
                    {
                        position.X = CONTINUE_SAVES_NAME_XPOS;
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, NO_SAVES_MESSAGE, position * texSize, CONTINUE_SAVES_TEXT_COLOR, 0, Vector2.Zero, NO_SAVES_SCALE, SpriteEffects.None, 0);
                    }
                    break;
                case SettingsMode.Setup:
                    float letterHeight = RetroGame.FONT_PIXEL_LARGE.MeasureString("M").Y;
                    float letterSizeOffset = (letterHeight / folderRenderTarget.Height) / 2;
                    position = new Vector2(SETUP_XPOS, SETUP_YPOS);
                    spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, "#" + Hero.NEW_HERO_IDS[currentSetupIndex].ToString("0000"), position * texSize, Color.Black, 0, new Vector2(0, letterHeight / 2), 1, SpriteEffects.None, 0);
                    position.Y += MENU_POSITION_VERTICAL_STEP;
                    if (setupTypingName)
                    {
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, setupTypingInstructions, SETUP_TYPING_INSTRUCTIONS_POS * texSize, Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, setupTypedName + "_", position * texSize, Color.Black, 0, new Vector2(0, letterHeight / 2), 1, SpriteEffects.None, 0);
                    }
                    else
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_LARGE, Hero.NEW_HERO_NAMES[currentSetupIndex], position * texSize, Color.Black, 0, new Vector2(0, letterHeight / 2), 1, SpriteEffects.None, 0);
                    position.Y += MENU_POSITION_VERTICAL_STEP;

                    const float OUTER_SHRINKAGE_Y = 0.10f;
                    const float INNER_SHRINKAGE_X = 0.025f;
                    const float INNER_SHRINKAGE_Y = 0.25f;
                    Vector2 outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    Vector2 innerBarPos = new Vector2(position.X + (COLOR_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    Vector2 outerBarSize = new Vector2(COLOR_BAR_WIDTH, MENU_POSITION_VERTICAL_STEP * (1 - OUTER_SHRINKAGE_Y * 2)) * texSize;
                    Vector2 innerBarSize = new Vector2(COLOR_BAR_WIDTH * (1 - INNER_SHRINKAGE_X * 2), MENU_POSITION_VERTICAL_STEP * (1 - INNER_SHRINKAGE_Y * 2)) * texSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, COLOR_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    float colorPerc = Hero.NEW_HERO_COLORS[currentSetupIndex].R / 255f;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * colorPerc), (int)innerBarSize.Y), null, Color.Red, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    position.Y += MENU_POSITION_VERTICAL_STEP;
                    
                    outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    innerBarPos = new Vector2(position.X + (COLOR_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, COLOR_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    colorPerc = Hero.NEW_HERO_COLORS[currentSetupIndex].G / 255f;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * colorPerc), (int)innerBarSize.Y), null, Color.Green, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    position.Y += MENU_POSITION_VERTICAL_STEP;
                    
                    outerBarPos = new Vector2(position.X, position.Y) * texSize;
                    innerBarPos = new Vector2(position.X + (COLOR_BAR_WIDTH * INNER_SHRINKAGE_X), position.Y) * texSize;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)outerBarPos.X, (int)outerBarPos.Y, (int)outerBarSize.X, (int)outerBarSize.Y), null, COLOR_BAR_COLOR_OUTER, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    colorPerc = Hero.NEW_HERO_COLORS[currentSetupIndex].B / 255f;
                    spriteBatchHUD.Draw(RetroGame.PIXEL, new Rectangle((int)innerBarPos.X, (int)innerBarPos.Y, (int)(innerBarSize.X * colorPerc), (int)innerBarSize.Y), null, Color.Blue, 0, new Vector2(0, 0.5f), SpriteEffects.None, 0);
                    break;
                case SettingsMode.Credits:
                    position = new Vector2(CREDITS_LEFT_ALIGN, CREDITS_TOP_YPOS);
                    if (currentMenuIndex < NUM_CREDITS_PAGES)
                        currentCreditsPage = currentMenuIndex;
                    for(int i = 0; i < CREDITS_LINES[currentCreditsPage].Length; i++)
                    {
                        string line = CREDITS_LINES[currentCreditsPage][i];
                        bool highlighted = CREDITS_HIGHLIGHTS[currentCreditsPage][i];
                        spriteBatchHUD.DrawString(RetroGame.FONT_PIXEL_SMALL, line, position * texSize, highlighted ? CREDITS_HIGHLIGHT_COLOR : CREDITS_TITLE_COLOR, 0, Vector2.Zero, CREDITS_LINE_SCALE, SpriteEffects.None, 0);
                        position.Y += CREDITS_LINE_SPACING;
                    }
                    break;
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
            spriteBatchHUD.End();
        }
    }
}