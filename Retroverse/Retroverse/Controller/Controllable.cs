using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public abstract class Controllable
    {
        public const float DRAW_BINDING_SCALE_KEYBOARD = 0.6f;
        public const float DRAW_KEYBOARD_PERCENTAGE_OFFSET = 0.05f;
        public const float DRAW_BINDING_SCALE_XBOX = 0.9f;

        public static readonly float TRIGGER_THRESHOLD = 0.1f;
        public static readonly float STICK_THRESHOLD = 0.1f;

        public Direction controllerDirection;
        public Vector2 dirVector
        {
            get { return controllerDirection.toVector(); }
            private set { }
        }

        public InputType currentInputType = InputType.Keyboard;

        public Bindings currentBindings;

        public void UpdateControls(Bindings bindings, GameTime gameTime)
        {
            currentBindings = bindings;

            bindings.prevStatePad = bindings.statePad;
            bindings.prevStateKey = bindings.stateKey;
            GamePadState prevStatePad = bindings.prevStatePad;
            KeyboardState prevStateKey = bindings.prevStateKey;
            GamePadState statePad = GamePad.GetState(bindings.PlayerIndex);
            KeyboardState stateKey = Keyboard.GetState();
            bindings.statePad = statePad;
            bindings.stateKey = stateKey;
            bool updatedOnce = bindings.updatedOnce;

            if (bindings.onNextInputAction != null)
            {
                Buttons? pressedButton = null;
                Keys? pressedKey = null;
                foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
                {
                    if (statePad.IsButtonDown(button) && !currentBindings.prevStatePad.IsButtonDown(button))
                    {
                        pressedButton = button;
                        break;
                    }
                }
                for (int i = 0; i < stateKey.GetPressedKeys().Length; i++)
                {
                    if (!currentBindings.prevStateKey.IsKeyDown(stateKey.GetPressedKeys()[i]))
                    {
                        pressedKey = stateKey.GetPressedKeys()[0];
                        break;
                    }
                }
                if (pressedKey != null || pressedButton != null)
                {
                    Action<Keys?, Buttons?> nextInputAction = bindings.onNextInputAction;
                    bindings.onNextInputAction = null; //nullify field before the action to allow the action to create an input capture loop
                    nextInputAction(pressedKey, pressedButton);
                    return;
                }
            }

            bool keyboardUsed = false;
            bool gamepadUsed = false;

            Dictionary<Buttons, InputAction> gamepadBindings = bindings.getGamepadBindings();
            Dictionary<Keys, InputAction> keyboardBindings = bindings.getKeyboardBindings();
            foreach (KeyValuePair<Buttons, InputAction> pair in gamepadBindings)
            {
                if (statePad.IsButtonDown(pair.Key))
                {
                    gamepadUsed = true;
                    OnInputAction(pair.Value, updatedOnce && !currentBindings.prevStatePad.IsButtonDown(pair.Key));
                }
            }
            foreach (KeyValuePair<Keys, InputAction> pair in keyboardBindings)
            {
                if (stateKey.IsKeyDown(pair.Key))
                {
                    keyboardUsed = true;
                    OnInputAction(pair.Value, updatedOnce && !currentBindings.prevStateKey.IsKeyDown(pair.Key));
                }
            }

            //directional movement -- priority to figure out which controller to "listen" to: Keyboard > D-pad > Analog stick
            controllerDirection = Direction.None;
            if (Math.Abs(statePad.ThumbSticks.Left.X) > Math.Abs(statePad.ThumbSticks.Left.Y)) //analog stick block
            {
                if (Math.Abs(statePad.ThumbSticks.Left.X) > STICK_THRESHOLD)
                    if (statePad.ThumbSticks.Left.X > 0)
                        controllerDirection = Direction.Right;
                    else
                        controllerDirection = Direction.Left;
            }
            else
            {
                if (Math.Abs(statePad.ThumbSticks.Left.Y) > STICK_THRESHOLD)
                    if (statePad.ThumbSticks.Left.Y > 0)
                        controllerDirection = Direction.Up;
                    else
                        controllerDirection = Direction.Down;
            }
            if (statePad.ThumbSticks.Left != Vector2.Zero || currentBindings.statePad.ThumbSticks.Right != Vector2.Zero)
            {
                gamepadUsed = true;
            }
            bool dpadUsed = true;
            if (statePad.IsButtonDown(Buttons.DPadUp)) //dpad block
                controllerDirection = Direction.Up;
            else if (statePad.IsButtonDown(Buttons.DPadDown))
                controllerDirection = Direction.Down;
            else if (statePad.IsButtonDown(Buttons.DPadLeft))
                controllerDirection = Direction.Left;
            else if (statePad.IsButtonDown(Buttons.DPadRight))
                controllerDirection = Direction.Right;
            else
                dpadUsed = false;
            gamepadUsed = gamepadUsed || dpadUsed;
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds; //WASD block
            Keys[] movementKeys = bindings.MovementKeys;
            Dictionary<Keys, double> lastKeyPresses = bindings.LastTimeMovementKeysWerePressed;
            foreach (Keys k in movementKeys)
            {
                if (stateKey.IsKeyDown(k))
                {
                    keyboardUsed = true;
                    if (lastKeyPresses[k] < 0)
                        lastKeyPresses[k] = currentTime;
                }
                else
                {
                    lastKeyPresses[k] = -1;
                }
            }
            KeyValuePair<Keys, double> lastPressedKeyPair = new KeyValuePair<Keys, double>(Keys.Escape, -1);
            foreach (KeyValuePair<Keys, double> pair in lastKeyPresses)
            {
                if (pair.Value > lastPressedKeyPair.Value)
                    lastPressedKeyPair = pair;
            }
            if (keyboardBindings.ContainsKey(lastPressedKeyPair.Key))
                switch (keyboardBindings[lastPressedKeyPair.Key])
                {
                    case InputAction.Up:
                        controllerDirection = Direction.Up;
                        break;
                    case InputAction.Down:
                        controllerDirection = Direction.Down;
                        break;
                    case InputAction.Left:
                        controllerDirection = Direction.Left;
                        break;
                    case InputAction.Right:
                        controllerDirection = Direction.Right;
                        break;
                }

            if (keyboardUsed)
                currentInputType = InputType.Keyboard;
            else if (gamepadUsed)
                currentInputType = InputType.Gamepad;
            
            bindings.updatedOnce = true;
        }

        public abstract void OnInputAction(InputAction action, bool pressedThisFrame);

        public bool isDown(Keys key)
        {
            return (currentBindings != null) && currentBindings.stateKey.IsKeyDown(key);
        }

        public bool isDown(Buttons button)
        {
            return (currentBindings != null) && currentBindings.statePad.IsButtonDown(button);
        }

        public bool pressedThisFrame(Keys key)
        {
            return (currentBindings != null) && !currentBindings.prevStateKey.IsKeyDown(key) && currentBindings.stateKey.IsKeyDown(key);
        }

        public bool pressedThisFrame(Buttons button)
        {
            return (currentBindings != null) && !currentBindings.prevStatePad.IsButtonDown(button) && currentBindings.statePad.IsButtonDown(button);
        }

        public bool releasedThisFrame(Keys key)
        {
            return (currentBindings != null) && currentBindings.prevStateKey.IsKeyDown(key) && !currentBindings.stateKey.IsKeyDown(key);
        }

        public bool releasedThisFrame(Buttons button)
        {
            return (currentBindings != null) && currentBindings.prevStatePad.IsButtonDown(button) && !currentBindings.statePad.IsButtonDown(button);
        }

        public void DrawBinding(SpriteBatch spriteBatch, InputAction action, Vector2 position, Vector2 origin, float relativeScale)
        {
            if (currentBindings == null)
                return;
            SpriteFont font = null;
            float drawBindingScale = 0f;
            origin.Y = 0.5f;
            switch (currentInputType)
            {
                case InputType.Gamepad:
                    drawBindingScale = DRAW_BINDING_SCALE_XBOX;
                    font = RetroGame.FONT_HUD_XBOX;
                    break;
                case InputType.Keyboard:
                    drawBindingScale = DRAW_BINDING_SCALE_KEYBOARD;
                    font = RetroGame.FONT_HUD_KEYS;
                    origin.Y -= DRAW_KEYBOARD_PERCENTAGE_OFFSET;
                    break;
            }
            string bindingChar = currentBindings.getHUDIconCharacter(currentInputType, action);
            spriteBatch.DrawString(font, bindingChar, position, Color.White, 0, origin * font.MeasureString(bindingChar), drawBindingScale * relativeScale, SpriteEffects.None, 0);
        }
    }
}
