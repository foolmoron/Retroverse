using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class StaticTransitionScreen : Screen
    {
        public const float STATIC_WHITENESS = 0.5f;

        public float staticTransitionTime;
        public float staticTime = 0;
        public int staticTimeModifier = 1;
        public byte staticAlpha;
        public Action onTransitionFinished;
        public bool automaticallyPopWhenFinished;

        public RenderTarget2D staticTransitionRenderTarget;
        public SpriteBatch spriteBatch;

        private int transitionDirection;
        public TransitionMode Mode { get { return (transitionDirection > 0) ? TransitionMode.ToStatic : TransitionMode.FromStatic; } }

        public StaticTransitionScreen(TransitionMode mode, float transitionTime, Action onTransitionFinished, bool automaticallyPopWhenFinished = true)
        {
            DrawPreviousScreen = true;
            staticTransitionTime = transitionTime;
            transitionDirection = (mode == TransitionMode.ToStatic) ? 1 : -1;
            staticAlpha = (mode == TransitionMode.ToStatic) ? (byte)0 : byte.MaxValue;
            this.onTransitionFinished = onTransitionFinished;
            this.automaticallyPopWhenFinished = automaticallyPopWhenFinished;
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            staticTransitionRenderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
        }

        public override void OnScreenSizeChanged()
        {
            staticTransitionRenderTarget.Dispose();
            staticTransitionRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            staticTime += seconds;

            float interp = staticTime / staticTransitionTime;
            if (Mode == TransitionMode.ToStatic)
                staticAlpha = (byte)(255 * interp);
            else if (Mode == TransitionMode.FromStatic)
                staticAlpha = (byte)(255 * (1 - interp));

            if (interp >= 1)
            {
                if(automaticallyPopWhenFinished)
                    RetroGame.PopScreen();
                if (onTransitionFinished != null)
                    onTransitionFinished();
            }
        }

        public override void PreDraw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(staticTransitionRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Effect staticEffect = Effects.RewindRandomStatic;
            staticEffect.CurrentTechnique = staticEffect.Techniques["CreateStatic"];
            staticEffect.Parameters["randomSeed"].SetValue((float)RetroGame.rand.NextDouble());
            staticEffect.Parameters["numLinesOfStatic"].SetValue(1);
            staticEffect.Parameters["staticPositions"].SetValue(new float[1] { 0.5f });
            staticEffect.Parameters["staticThicknesses"].SetValue(new float[1] { 1 });
            staticEffect.Parameters["whiteness"].SetValue(STATIC_WHITENESS);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                                 DepthStencilState.None, RasterizerState.CullCounterClockwise,
                                 staticEffect, Matrix.Identity);
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(0, 0, staticTransitionRenderTarget.Width, staticTransitionRenderTarget.Height), Color.White);
            spriteBatch.End();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatch.Draw(staticTransitionRenderTarget, Vector2.Zero, Color.White.withAlpha(staticAlpha));
            spriteBatch.End();
        }

        public override void Dispose()
        {
            spriteBatch.Dispose();
            staticTransitionRenderTarget.Dispose();
        }
    }
}
