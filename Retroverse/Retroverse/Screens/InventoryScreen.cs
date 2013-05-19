using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class InventoryScreen : Screen
    {
        public readonly int activePlayerIndex;
        public Bindings bindingsOne;
        public Bindings bindingsTwo;
        public int currentPlayerControls = -1;
        public RenderTarget2D inventoryBarRenderTarget;
        public SpriteBatch spriteBatchHUD;

        public InventoryScreen(Bindings activeBindings)
        {
            DrawPreviousScreen = true;

            bindingsOne = RetroGame.getHeroes()[0].bindings;
            if (RetroGame.NUM_PLAYERS > 1)
                bindingsTwo = RetroGame.getHeroes()[1].bindings;

            if (bindingsOne == activeBindings)
                activePlayerIndex = Player.One;
            else
                activePlayerIndex = Player.Two;

            Inventory.Reset();
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            spriteBatchHUD = new SpriteBatch(GraphicsDevice);
            inventoryBarRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            Inventory.warning = "";
            Inventory.warnedIcons = null;
        }

        public override void OnScreenSizeChanged()
        {
            Initialize(GraphicsDevice);
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            if (pressedThisFrame)
            {
                switch (action)
                {
                    case InputAction.Up:
                    case InputAction.Down:
                    case InputAction.Left:
                    case InputAction.Right:
                        Inventory.UpdateCursorPosition(currentPlayerControls, action);
                        break;
                    case InputAction.Start:
                    case InputAction.Escape:
                        if (currentPlayerControls == activePlayerIndex)
                        {
                            SoundManager.PlaySoundOnce("ButtonBack");
                            RetroGame.PopScreen();
                        }
                        break;
                    case InputAction.Action1:
                        Inventory.SelectWithCursor(currentPlayerControls);
                        break;
                    case InputAction.Action2:
                        Inventory.GoBack(currentPlayerControls, currentPlayerControls == activePlayerIndex);
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            currentPlayerControls = Player.One;
            UpdateControls(bindingsOne, gameTime);
            if (bindingsTwo != null)
            {
                currentPlayerControls = Player.Two;
                UpdateControls(bindingsTwo, gameTime);
            }
            Inventory.UpdateCursorBobAnimation(gameTime);
            Inventory.UpdateWarning(gameTime);
        }


        public override void PreDraw(GameTime gameTime) { }

        public override void Draw(GameTime gameTime)
        {
            spriteBatchHUD.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Inventory.DrawStorage(spriteBatchHUD);
            spriteBatchHUD.End();
        }

        public override void Dispose()
        {
            spriteBatchHUD.Dispose();
            inventoryBarRenderTarget.Dispose();
        }
    }
}
