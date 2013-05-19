using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class RetroPortScreen : Screen
    {
        public Bindings bindings;
        public InputAction cancelAction;
        public Hero controllingHero;

        //dummy screen that doesn't draw anything, just waits for a cancel action and disables all other input
        public RetroPortScreen(Hero controllingHero, InputAction cancelAction)
        {
            DrawPreviousScreen = true;

            bindings = controllingHero.bindings;
            this.controllingHero = controllingHero;
            this.cancelAction = cancelAction;
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
        }

        public override void OnScreenSizeChanged()
        {
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            if (pressedThisFrame)
            {
                if (action == cancelAction)
                {
                    History.CancelRevert();
                    return;
                }

                switch (action)
                {
                    case InputAction.Start:
                        RetroGame.PauseGame(controllingHero);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            RiotGuardWall.UpdateRetro(gameTime);
            RetroGame.TopLevelManagerScreen.levelManager.UpdateRetro(gameTime);
            History.UpdateReverse(gameTime);
            UpdateControls(bindings, gameTime);
        }

        public override void PreDraw(GameTime gameTime) { }
        public override void Draw(GameTime gameTime) { }
        public override void Dispose() { }
    }
}
