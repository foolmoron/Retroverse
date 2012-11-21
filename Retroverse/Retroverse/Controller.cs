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

namespace Retroverse
{
    public enum Direction { None, Up, Down, Left, Right };
    public enum InputType { Gamepad, Keyboard };

    public static class Controller
    {
        public static readonly float TRIGGER_THRESHOLD = 0.1f;
        public static readonly float STICK_THRESHOLD = 0.1f;

        public static GamePadState statePad;
        public static KeyboardState stateKey;
        public static GamePadState prevStatePad;
        public static KeyboardState prevStateKey;

        public static InputType currentInputType = InputType.Gamepad;

        public static Direction direction;
        public static Vector2 dirVector
        {
            get { return DIR_TO_VECTOR[direction]; }
            private set { }
        }

        private static readonly Dictionary<Direction, Vector2> DIR_TO_VECTOR = new Dictionary<Direction, Vector2>(){
            {Direction.None, Vector2.Zero},
            {Direction.Up, new Vector2(0, -1)},
            {Direction.Down, new Vector2(0, 1)},
            {Direction.Left, new Vector2(-1, 0)},
            {Direction.Right, new Vector2(1, 0)},
        };

        private static readonly Keys[] MOVEMENT_KEYS = new Keys[] { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Up, Keys.Down, Keys.Left, Keys.Right};
        private static readonly Dictionary<Keys, double> LAST_TIME_KEY_WAS_PRESSED = new Dictionary<Keys, double>()
        {
            {Keys.W, -1},
            {Keys.A, -1},
            {Keys.S, -1},
            {Keys.D, -1},
        };

        public static readonly Dictionary<Buttons, Action<bool>> gamepadButtons = new Dictionary<Buttons, Action<bool>>()
        {
            {Buttons.A, action1},
            {Buttons.B, action3},
            {Buttons.RightShoulder, action2},
            {Buttons.X, action5},
            {Buttons.Y, action4},
            {Buttons.Start, startButton},
        };
        public static readonly Dictionary<Keys, Action<bool>> keyboardButtons = new Dictionary<Keys, Action<bool>>()
        {
            {Keys.Space, action1},
            {Keys.LeftShift, action2},
            {Keys.LeftControl, action3},
            {Keys.LeftAlt, action4},
            {Keys.Enter, startButton},
            {Keys.Escape, startButton},
            {Keys.Q, action5},
            {Keys.Z, action4},
            {Keys.X, action5},
            {Keys.C, action6},
            {Keys.J, action7},
        };

        public static void Update(GameTime gameTime)
        {
            statePad = GamePad.GetState(PlayerIndex.One);
            stateKey = Keyboard.GetState();

            bool keyboardUsed = false;
            bool gamepadUsed = false;

            //gamepad buttons
            foreach (KeyValuePair<Buttons, Action<bool>> pair in gamepadButtons)
            {
                if (statePad.IsButtonDown(pair.Key))
                {
                    gamepadUsed = true;
                    pair.Value(!prevStatePad.IsButtonDown(pair.Key));
                }
            }
            //keyboard buttons
            foreach (KeyValuePair<Keys, Action<bool>> pair in keyboardButtons)
            {
                if (stateKey.IsKeyDown(pair.Key))
                {
                    keyboardUsed = true;
                    pair.Value(!prevStateKey.IsKeyDown(pair.Key));
                }
            }
            //gamepad triggers
            if (statePad.Triggers.Right > TRIGGER_THRESHOLD && !(statePad.Triggers.Right > TRIGGER_THRESHOLD))
            {
                // right trigger
                gamepadUsed = true;
            }
            if (statePad.Triggers.Left > TRIGGER_THRESHOLD && !(statePad.Triggers.Left > TRIGGER_THRESHOLD))
            {
                // left trigger
                gamepadUsed = true;
            }
            //directional movement -- priority to figure out which controller to "listen" to: Keyboard > D-pad > Analog stick
            direction = Direction.None;
            if (Math.Abs(statePad.ThumbSticks.Left.X) > Math.Abs(statePad.ThumbSticks.Left.Y)) //analog stick block
            {
                if (Math.Abs(statePad.ThumbSticks.Left.X) > STICK_THRESHOLD)
                    if (statePad.ThumbSticks.Left.X > 0)
                        direction = Direction.Right;
                    else
                        direction = Direction.Left;
            }
            else
            {
                if (Math.Abs(statePad.ThumbSticks.Left.Y) > STICK_THRESHOLD)
                    if (statePad.ThumbSticks.Left.Y > 0)
                        direction = Direction.Up;
                    else
                        direction = Direction.Down;
            }
            if (statePad.ThumbSticks.Left != Vector2.Zero || statePad.ThumbSticks.Right != Vector2.Zero)
            {
                gamepadUsed = true;
            }
            bool dpadUsed = true;
            if (statePad.IsButtonDown(Buttons.DPadUp)) //dpad block
                direction = Direction.Up;
            else if (statePad.IsButtonDown(Buttons.DPadDown))
                direction = Direction.Down;
            else if (statePad.IsButtonDown(Buttons.DPadLeft))
                direction = Direction.Left;
            else if (statePad.IsButtonDown(Buttons.DPadRight))
                direction = Direction.Right;
            else
                dpadUsed = false;
            gamepadUsed = gamepadUsed || dpadUsed;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds; //WASD block
            foreach (Keys k in MOVEMENT_KEYS)
            {
                if (stateKey.IsKeyDown(k))
                {
                    keyboardUsed = true;
                    if (LAST_TIME_KEY_WAS_PRESSED[k] < 0)
                        LAST_TIME_KEY_WAS_PRESSED[k] = currentTime;
                }
                else
                {
                    LAST_TIME_KEY_WAS_PRESSED[k] = -1;
                }
            }
            KeyValuePair<Keys, double> lastPressedKeyPair = new KeyValuePair<Keys,double>(Keys.Escape, -1);
            foreach (KeyValuePair<Keys, double> pair in LAST_TIME_KEY_WAS_PRESSED)
            {
                if (pair.Value > lastPressedKeyPair.Value)
                    lastPressedKeyPair = pair;
            }
            switch (lastPressedKeyPair.Key)
            {
                case Keys.W:
                case Keys.Up:
                    direction = Direction.Up;
                    break;
                case Keys.A:
                case Keys.Left:
                    direction = Direction.Left;
                    break;
                case Keys.S:
                case Keys.Down:
                    direction = Direction.Down;
                    break;
                case Keys.D:
                case Keys.Right:
                    direction = Direction.Right;
                    break;
            }

#if DEBUG
            // debug powerup options
            if (pressed(Keys.Y))
                Hero.instance.powerupBoost = (Hero.instance.powerupBoost + 1) % 3;
            else if (pressed(Keys.U))
                Hero.instance.powerupDrill = (Hero.instance.powerupDrill + 1) % 3;
            else if (pressed(Keys.I))
                Hero.instance.powerupGun = (Hero.instance.powerupGun + 1) % 4;
            else if (pressed(Keys.O))
                Hero.instance.powerupRetro = (Hero.instance.powerupRetro + 1) % 3;
            else if (pressed(Keys.P))
                Hero.instance.powerupRadar = (Hero.instance.powerupRadar + 1) % 2;
            else
#endif
                if (pressed(Keys.L))
                    Game1.toggleScreenSize();
            if (pressed(Keys.F12))
                Game1.gameOver();

            if (keyboardUsed)
                currentInputType = InputType.Keyboard;
            else if (gamepadUsed)
                currentInputType = InputType.Gamepad;

            prevStateKey = stateKey;
            prevStatePad = statePad;
        }

        public static bool isDown(Keys key)
        {
            return stateKey.IsKeyDown(key);
        }

        public static bool isDown(Buttons button)
        {
            return statePad.IsButtonDown(button);
        }

        public static bool pressed(Keys key)
        {
            return !prevStateKey.IsKeyDown(key) && stateKey.IsKeyDown(key);
        }

        public static bool pressed(Buttons button)
        {
            return !prevStatePad.IsButtonDown(button) && statePad.IsButtonDown(button);
        }

        public static bool released(Keys key)
        {
            return prevStateKey.IsKeyDown(key) && !stateKey.IsKeyDown(key);
        }

        public static bool released(Buttons button)
        {
            return prevStatePad.IsButtonDown(button) && !statePad.IsButtonDown(button);
        }

        public static void startButton(bool pressedDownThisFrame) //enter, start
        {
            if (pressedDownThisFrame)
            {
                Game1.pressStartButton();
            }
        }

        public static void action1(bool pressedDownThisFrame) //space, A
        {
            if (pressedDownThisFrame)
            {
            }
            Hero.instance.fire();
        }

        public static void action2(bool pressedDownThisFrame) //shift, B
        {
            if (pressedDownThisFrame)
            {
            }
            Hero.instance.burst();  
        }

        public static void action3(bool pressedDownThisFrame) //ctrl, RB
        {
            if (pressedDownThisFrame)
            {
            }
        }

        public static void action4(bool pressedDownThisFrame) //alt, X, Y
        {
            if (pressedDownThisFrame)
            {
            }
        }

        public static void action5(bool pressedDownThisFrame)
        {
            if (pressedDownThisFrame)
            {
                Hero.instance.activateRetro();
            }
        }

        public static void action6(bool pressedDownThisFrame)
        {
            if (pressedDownThisFrame)
            {
                Hero.instance.special2();
            }
        }

        public static void action7(bool pressedDownThisFrame)
        {
            Hero.instance.burst();            
        }

        public static string getKeyIconForPowerup(int i)
        {
            switch (i)
            {
                case 1:
                    return "^";
                case 2:
                    return "_";
                case 3:
                    return "Q";
                case 4:
                    return "";
                case 5:
                    return "";
                default:
                    return "";
            }
        }

        public static string getButtonIconForPowerup(int i)
        {

            switch (i)
            {
                case 1:
                    return "R";
                case 2:
                    return "A";
                case 3:
                    return "X";
                case 4:
                    return "";
                case 5:
                    return "";
                default:
                    return "";
            }
        }
    }
}