using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class Bindings
    {
        public const string BINDING_FILE_WARNING = "*This file contains key bindings. DO NOT DELETE OR MODIFY!*";
        public const string BINDING_FILE_DIRECTORY = @"Content\DefaultBindings\";
        public const int PAD_LENGTH = 20;
        public static readonly Dictionary<PlayerIndex, string> DEFAULT_BINDING_FILENAMES = new Dictionary<PlayerIndex, string>()
        {
            {PlayerIndex.One, "defaultplayer1.bindings"},            
            {PlayerIndex.Two, "defaultplayer2.bindings"}
        };
        public static readonly Dictionary<PlayerIndex, List<Binding>> DEFAULT_BINDINGS = new Dictionary<PlayerIndex, List<Binding>>()
        {
            {PlayerIndex.One, new List<Binding>()
                {
                    new Binding(InputAction.Up, Keys.W, Buttons.DPadUp),
                    new Binding(InputAction.Down, Keys.S, Buttons.DPadDown),
                    new Binding(InputAction.Left, Keys.A, Buttons.DPadLeft),
                    new Binding(InputAction.Right, Keys.D, Buttons.DPadRight),
                    new Binding(InputAction.Action1, Keys.Space, Buttons.A),
                    new Binding(InputAction.Action2, Keys.Q, Buttons.B),
                    new Binding(InputAction.Action3, Keys.LeftShift, Buttons.X),
                    new Binding(InputAction.Action4, Keys.LeftControl, Buttons.Y),
                    new Binding(InputAction.Start, Keys.T, Buttons.Start),
                }
            },            
            {PlayerIndex.Two, new List<Binding>()
                {
                    new Binding(InputAction.Up, Keys.NumPad8, Buttons.DPadUp),
                    new Binding(InputAction.Down, Keys.NumPad5, Buttons.DPadDown),
                    new Binding(InputAction.Left, Keys.NumPad4, Buttons.DPadLeft),
                    new Binding(InputAction.Right, Keys.NumPad6, Buttons.DPadRight),
                    new Binding(InputAction.Action1, Keys.NumPad0, Buttons.A),
                    new Binding(InputAction.Action2, Keys.NumPad9, Buttons.B),
                    new Binding(InputAction.Action3, Keys.Enter, Buttons.X),
                    new Binding(InputAction.Action4, Keys.Add, Buttons.Y),
                    new Binding(InputAction.Start, Keys.Multiply, Buttons.Start),
                }
            }
        };
        public static readonly Dictionary<PlayerIndex, List<Binding>> CUSTOM_BINDINGS = new Dictionary<PlayerIndex, List<Binding>>()
        {
            {PlayerIndex.One, null},
            {PlayerIndex.Two, null},
        };
        public static readonly Dictionary<PlayerIndex, bool> USE_CUSTOM_BINDINGS = new Dictionary<PlayerIndex, bool>()
        {
            {PlayerIndex.One, false},
            {PlayerIndex.Two, false},
        };

        #region bindable buttons/keys lists
        public static readonly Buttons[] BINDABLE_BUTTONS = new Buttons[]
        {
            Buttons.A,
            Buttons.B,
            Buttons.X,
            Buttons.Y,
            Buttons.RightStick,
            Buttons.LeftStick,
            Buttons.RightShoulder,
            Buttons.LeftShoulder,
            Buttons.RightTrigger,
            Buttons.LeftTrigger,
            Buttons.Start,
        };
        public static readonly Keys[] BINDABLE_KEYS = new Keys[]
        {
            Keys.A,
            Keys.Add,
            Keys.B,
            //Keys.Back,
            Keys.C,
            //Keys.CapsLock,
            Keys.D,
            Keys.Decimal,
            Keys.Delete,
            Keys.Divide,
            Keys.Down,
            Keys.E,
            //Keys.End,
            Keys.Enter,
            //Keys.Escape,
            Keys.F,
            Keys.G,
            Keys.H,
            //Keys.Home,
            Keys.I,
            //Keys.Insert,
            Keys.J,
            Keys.K,
            Keys.L,
            Keys.Left,
            Keys.LeftAlt,
            Keys.LeftControl,
            Keys.LeftShift,
            Keys.M,
            Keys.Multiply,
            Keys.N,
            Keys.NumPad0,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.O,
            Keys.OemBackslash,
            Keys.OemCloseBrackets,
            Keys.OemComma,
            Keys.OemMinus,
            Keys.OemOpenBrackets,
            Keys.OemPeriod,
            Keys.OemPlus,
            Keys.OemQuotes,
            Keys.OemSemicolon,
            Keys.OemTilde,
            Keys.P,
            //Keys.PageDown,
            //Keys.PageUp,
            Keys.Q,
            Keys.R,
            Keys.Right,
            //Keys.RightAlt,
            //Keys.RightControl,
            //Keys.RightShift,
            Keys.S,
            Keys.Space,
            Keys.Subtract,
            Keys.T,
            //Keys.Tab,
            Keys.U,
            Keys.Up,
            Keys.V,
            Keys.W,
            Keys.X,
            Keys.Y,
            Keys.Z,
            Keys.D0,
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
        };
        #endregion

        public static readonly InputAction[] MOVEMENT_ACTIONS = new InputAction[4] { InputAction.Up, InputAction.Down, InputAction.Left, InputAction.Right };

        private readonly List<Keys> movementKeys = new List<Keys>();
        public Keys[] MovementKeys { get { return movementKeys.ToArray(); } private set { return; } }
        private readonly Dictionary<Keys, double> lastTimeMovementKeysWerePressed = new Dictionary<Keys, double>();
        public Dictionary<Keys, double> LastTimeMovementKeysWerePressed { get { return lastTimeMovementKeysWerePressed; } private set { return; } }

        public PlayerIndex PlayerIndex { get; private set; }
        private List<Binding> bindings = new List<Binding>();
        private Dictionary<Buttons, InputAction> buttonsToActions;
        private Dictionary<Keys, InputAction> keysToActions;

        public GamePadState statePad;
        public GamePadState prevStatePad;
        public KeyboardState stateKey;
        public KeyboardState prevStateKey;
        public bool updatedOnce = false;

        public Action<Keys?, Buttons?> onNextInputAction = null;

        public static void Load(Dictionary<PlayerIndex, List<Binding>> customBindings)
        {
            foreach (PlayerIndex playerIndex in new[] { PlayerIndex.One, PlayerIndex.Two })
            {
                string defaultBindingFilename = DEFAULT_BINDING_FILENAMES[playerIndex];
                string fullFilename = BINDING_FILE_DIRECTORY + defaultBindingFilename;
                Directory.CreateDirectory(BINDING_FILE_DIRECTORY);
                if (!File.Exists(fullFilename))
                    saveBindingsToFile(DEFAULT_BINDINGS[playerIndex], File.Create(fullFilename));
                DEFAULT_BINDINGS[playerIndex] = loadFromBindingFile(defaultBindingFilename);
                if(customBindings[playerIndex] != null)
                    CUSTOM_BINDINGS[playerIndex] = customBindings[playerIndex];
                else
                    CUSTOM_BINDINGS[playerIndex] = new List<Binding>(DEFAULT_BINDINGS[playerIndex]);
            }
        }

        public Bindings(PlayerIndex playerIndex, string bindingFileName = null)
        {
            PlayerIndex = playerIndex;
            if (USE_CUSTOM_BINDINGS[playerIndex])
                setToCustom();
            else
                setToDefault();
            if (bindingFileName != null)
                setBindingList(loadFromBindingFile(bindingFileName));
        }

        private static List<Binding> loadFromBindingFile(string bindingFilename)
        {
            List<Binding> bindingList = new List<Binding>();
            string bindingFullFilename = BINDING_FILE_DIRECTORY + bindingFilename;
            if (File.Exists(bindingFullFilename))
            {
                using (StreamReader reader = new StreamReader(bindingFullFilename))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        try
                        {
                            string[] split = line.Split(' ');
                            InputAction action = (InputAction)Enum.Parse(typeof(InputAction), split[0], true);
                            Keys key = 0;
                            Buttons button = 0;
                            for (int i = 1; i < split.Length; i++)
                            {
                                if (!split[i].Contains('.'))
                                    continue;
                                string type = split[i].Split('.')[0];
                                string value = split[i].Split('.')[1];
                                if (type == "Keys")
                                {
                                    if (Enum.IsDefined(typeof(Keys), value))
                                    {
                                        key = (Keys)Enum.Parse(typeof(Keys), value, true);
                                    }
                                }
                                else if (type == "Buttons")
                                {
                                    if (Enum.IsDefined(typeof(Buttons), value))
                                    {
                                        button = (Buttons)Enum.Parse(typeof(Buttons), value, true);
                                    }
                                }
                            }
                            bindingList.Add(new Binding(action, key, button));
                        }
                        catch (Exception e) { }
                        finally
                        {
                            line = reader.ReadLine();
                        }
                    }
                }
            }
            return bindingList;
        }

        private void setBindingList(List<Binding> bindingList)
        {
            bindings = bindingList;
            movementKeys.Clear();
            lastTimeMovementKeysWerePressed.Clear();
            foreach(Binding bind in bindingList)
            {
                if (MOVEMENT_ACTIONS.Contains(bind.Action))
                {
                    movementKeys.Add(bind.Key);
                    lastTimeMovementKeysWerePressed.Add(bind.Key, -1);
                }
            }
            keysToActions = null;
            buttonsToActions = null;
        }

        public void setToDefault()
        {
            USE_CUSTOM_BINDINGS[PlayerIndex] = false;
            setBindingList(DEFAULT_BINDINGS[PlayerIndex]);
        }

        public void setToCustom()
        {
            USE_CUSTOM_BINDINGS[PlayerIndex] = true;
            setBindingList(CUSTOM_BINDINGS[PlayerIndex]);
        }

        private static void saveBindingsToFile(List<Binding> bindingsToSave, Stream fileStream)
        {
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(BINDING_FILE_WARNING);
                foreach (Binding bind in bindingsToSave)
                {
                    writer.WriteLine();
                    writer.Write(pad(bind.Action.ToString(), PAD_LENGTH));
                    writer.Write(pad("Keys." + bind.Key, PAD_LENGTH));
                    writer.Write(pad("Buttons." + bind.Button, PAD_LENGTH));
                }
            }
        }

        private static string pad(string toPad, int length, char padChar = ' ')
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                if (i < toPad.Length)
                    result += toPad[i];
                else
                    result += padChar;
            }
            return result;
        }

        public Dictionary<Buttons, InputAction> getGamepadBindings()
        {
            if (buttonsToActions == null)
            {
                buttonsToActions = new Dictionary<Buttons, InputAction>();
                foreach (Binding bind in bindings)
                {
                    buttonsToActions.Add(bind.Button, bind.Action);
                }
                //hardcode escape as back button
                buttonsToActions.Add(Buttons.Back, InputAction.Escape);
                //hardcode analog stick directions into button actions for menu
                buttonsToActions.Add(Buttons.LeftThumbstickUp, InputAction.Up);
                buttonsToActions.Add(Buttons.LeftThumbstickDown, InputAction.Down);
                buttonsToActions.Add(Buttons.LeftThumbstickLeft, InputAction.Left);
                buttonsToActions.Add(Buttons.LeftThumbstickRight, InputAction.Right);
            }
            return buttonsToActions;
        }

        public Dictionary<Keys, InputAction> getKeyboardBindings()
        {
            if (keysToActions == null)
            {
                keysToActions = new Dictionary<Keys, InputAction>();
                foreach (Binding bind in bindings)
                {
                    keysToActions.Add(bind.Key, bind.Action);
                }
                //hardcode escape as escape button
                keysToActions.Add(Keys.Escape, InputAction.Escape);
            }
            return keysToActions;
        }

        public static string GetHUDIconCharacter(Buttons button)
        {
            if (BINDABLE_BUTTONS.Contains(button))
                switch (button)
                {
                    case Buttons.A:
                        return " ";
                    case Buttons.B:
                        return "!";
                    case Buttons.X:
                        return "\"";
                    case Buttons.Y:
                        return "#";
                    case Buttons.RightStick:
                        return "$";
                    case Buttons.LeftStick:
                        return "%";
                    case Buttons.RightShoulder:
                        return "'";
                    case Buttons.LeftShoulder:
                        return "(";
                    case Buttons.RightTrigger:
                        return ")";
                    case Buttons.LeftTrigger:
                        return "*";
                    //case Buttons.Back:
                    //    return "+";
                    case Buttons.Start:
                        return ",";
                }
            return null;
        }

        public static string GetHUDIconCharacter(Keys key)
        {
            if (BINDABLE_KEYS.Contains(key))
                switch (key)
                {
                    case Keys.Add:
                        return "+";
                    //case Keys.CapsLock:
                    //    return "";
                    case Keys.Decimal:
                        return ".";
                    case Keys.Delete:
                        return "-";
                    case Keys.Divide:
                        return "/";
                    //case Keys.Down:
                    //    return "";
                    //case Keys.End:
                    //    return "";
                    case Keys.Enter:
                        return "\"";
                    //case Keys.Escape:
                    //    return "";
                    //case Keys.Home:
                    //    return "";
                    //case Keys.Insert:
                    //    return "";
                    //case Keys.LeftAlt:
                    //    return "";
                    case Keys.LeftControl:
                        return "~";
                    case Keys.LeftShift:
                        return "^";
                    case Keys.Multiply:
                        return "*";
                    case Keys.NumPad0:
                        return "0";
                    case Keys.NumPad1:
                        return "1";
                    case Keys.NumPad2:
                        return "2";
                    case Keys.NumPad3:
                        return "3";
                    case Keys.NumPad4:
                        return "4";
                    case Keys.NumPad5:
                        return "5";
                    case Keys.NumPad6:
                        return "6";
                    case Keys.NumPad7:
                        return "7";
                    case Keys.NumPad8:
                        return "8";
                    case Keys.NumPad9:
                        return "9";
                    case Keys.OemBackslash:
                        return "\\";
                    case Keys.OemCloseBrackets:
                        return "]";
                    case Keys.OemComma:
                        return ",";
                    case Keys.OemMinus:
                        return "-";
                    case Keys.OemOpenBrackets:
                        return "[";
                    case Keys.OemPeriod:
                        return ".";
                    case Keys.OemPlus:
                        return "=";
                    case Keys.OemQuotes:
                        return "'";
                    case Keys.OemSemicolon:
                        return ";";
                    case Keys.OemTilde:
                        return "`";
                    //case Keys.PageDown:
                    //    return "";
                    //case Keys.PageUp:
                    //    return "";
                    //case Keys.Right:
                    //    return "";
                    //case Keys.RightAlt:
                    //    return "";
                    //case Keys.RightControl:
                    //    return "";
                    case Keys.RightShift:
                        return "^";
                    case Keys.Space:
                        return "_";
                    case Keys.Subtract:
                        return "-";
                    //case Keys.Tab:
                    //    return "";
                    //case Keys.Up:
                    //    return "";
                    case Keys.D0:
                        return "0";
                    case Keys.D1:
                        return "1";
                    case Keys.D2:
                        return "2";
                    case Keys.D3:
                        return "3";
                    case Keys.D4:
                        return "4";
                    case Keys.D5:
                        return "5";
                    case Keys.D6:
                        return "6";
                    case Keys.D7:
                        return "7";
                    case Keys.D8:
                        return "8";
                    case Keys.D9:
                        return "9";
                    case Keys.Up:
                        return ":";
                    case Keys.Down:
                        return ">";
                    case Keys.Left:
                        return "<";
                    case Keys.Right:
                        return "?";
                    default:
                        return key.ToString();
                }
            return null;
        }

        public string getHUDIconCharacter(InputType currentInputType, InputAction action)
        {
            switch (currentInputType)
            {
                case InputType.Gamepad:
                    foreach (KeyValuePair<Buttons, InputAction> pair in getGamepadBindings())
                    {
                        if (pair.Value == action)
                        {
                            return GetHUDIconCharacter(pair.Key);
                        }
                    }
                    break;
                case InputType.Keyboard:
                    foreach (KeyValuePair<Keys, InputAction> pair in getKeyboardBindings())
                    {
                        if (pair.Value == action)
                        {
                            return GetHUDIconCharacter(pair.Key);
                        }
                    }
                    break;
            }
            return null;
        }

        public class Binding : IComparable<Binding>
        {
            public InputAction Action { get; private set; }
            public Buttons Button { get; private set; }
            public Keys Key { get; private set; }

            private Binding() { }

            public Binding(InputAction action)
            {
                Action = action;
                Key = 0;
                Button = 0;
            }

            public Binding(InputAction action, Keys key, Buttons button)
            {
                Action = action;
                Key = key;
                Button = button;
            }

            public int CompareTo(Binding other)
            {
                return (int)Action - (int)other.Action;
            }
        }
    }
}
