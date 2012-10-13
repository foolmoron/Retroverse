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

    public static class Controller
    {
        public static readonly float TRIGGER_THRESHOLD = 0.1f;
        public static readonly float STICK_THRESHOLD = 0.1f;

        public static GamePadState statePad;
        public static KeyboardState stateKey;
        public static GamePadState prevStatePad;
        public static KeyboardState prevStateKey;

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

        public static readonly Dictionary<Buttons, Action<bool>> gamepadButtons = new Dictionary<Buttons, Action<bool>>()
        {
            {Buttons.A, action1},
            {Buttons.B, action2},
            {Buttons.RightShoulder, action3},
            {Buttons.X, action4},
            {Buttons.Y, action4}
        };
        public static readonly Dictionary<Keys, Action<bool>> keyboardButtons = new Dictionary<Keys, Action<bool>>()
        {
            {Keys.Space, action1},
            {Keys.LeftShift, action2},
            {Keys.LeftControl, action3},
            {Keys.LeftAlt, action4},
            {Keys.Z, action4},
            {Keys.X, action5},
            {Keys.C, action6},
        };

        public static void Update(GameTime gameTime)
        {
            statePad = GamePad.GetState(PlayerIndex.One);
            stateKey = Keyboard.GetState();

            //gamepad buttons
            foreach (KeyValuePair<Buttons, Action<bool>> pair in gamepadButtons)
            {
                if (statePad.IsButtonDown(pair.Key))
                    pair.Value(!prevStatePad.IsButtonDown(pair.Key));
            }
            //keyboard buttons
            foreach (KeyValuePair<Keys, Action<bool>> pair in keyboardButtons)
            {
                if (stateKey.IsKeyDown(pair.Key))
                    pair.Value(!prevStateKey.IsKeyDown(pair.Key));
            }
            //gamepad triggers
            if (statePad.Triggers.Right > TRIGGER_THRESHOLD && !(statePad.Triggers.Right > TRIGGER_THRESHOLD))
            {
                // right trigger
            }
            if (statePad.Triggers.Left > TRIGGER_THRESHOLD && !(statePad.Triggers.Left > TRIGGER_THRESHOLD))
            {
                // left trigger
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
            if (statePad.IsButtonDown(Buttons.DPadUp)) //dpad block
                direction = Direction.Up;
            else if (statePad.IsButtonDown(Buttons.DPadDown))
                direction = Direction.Down;
            else if (statePad.IsButtonDown(Buttons.DPadLeft))
                direction = Direction.Left;
            else if (statePad.IsButtonDown(Buttons.DPadRight))
                direction = Direction.Right;
            if (stateKey.IsKeyDown(Keys.W)) //WASD block
                direction = Direction.Up;
            else if (stateKey.IsKeyDown(Keys.S))
                direction = Direction.Down;
            else if (stateKey.IsKeyDown(Keys.A))
                direction = Direction.Left;
            else if (stateKey.IsKeyDown(Keys.D))
                direction = Direction.Right;

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

        public static void action1(bool pressed) //space, A
        {
            if (pressed)
                    Hero.instance.spaceOrA();
            Hero.instance.fire();
        }

        public static void action2(bool pressed) //shift, B
        {
            if (pressed)
            {
                Hero.instance.shiftOrB();
                Game1.levelManager.scrolling = !Game1.levelManager.scrolling;
            }
        }

        public static void action3(bool pressed) //ctrl, RB
        {
            if (pressed)
            {
                Hero.instance.ctrlOrRB();
            }
        }

        public static void action4(bool pressed) //alt, X, Y
        {
            if (pressed)
            {
                Hero.instance.altOrXY();
                Game1.drawVignette = !Game1.drawVignette;
                Game1.drawEffects = !Game1.drawEffects;
            }
        }

        public static void action5(bool pressed)
        {
            if (pressed)
            {
                Hero.instance.special();
            }
        }

        public static void action6(bool pressed)
        {
            if (pressed)
            {
                Hero.instance.special2();
            }
        }
    }
}