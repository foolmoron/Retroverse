using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Particles;

namespace Retroverse
{
    public class ArenaCamera : Camera
    {
        public static float SCREENWIDTH_MIN = 800;
        public static float SCREENWIDTH_MAX = 1333;
        public float screenWidth;
        public static float SCREENHEIGHT_MIN = 480;
        public static float SCREENHEIGHT_MAX = 800;
        public float screenHeight;

        public static readonly float ZOOM_ESCAPE = 1.1f;
        public bool scrolling = false;

        public static readonly float SCROLL_SPEED_DEFAULT = 200f * 2f;
        public float scrollSpeed = SCROLL_SPEED_DEFAULT;
        public float scrollMultiplier = 1f;

        // intro "cutscene" values
        public static bool introFinished;
        public static readonly float INTRO_INITIAL_ZOOM = 0.9f;
        public static readonly float INTRO_FINAL_ZOOM = 1.03f;
        public static readonly float INTRO_ZOOM_VELOCITY = 0.15f;

        public ArenaCamera(Vector2 absoluteCenter)
        {
            screenWidth = SCREENWIDTH_MIN;
            screenHeight = MathHelper.Clamp(screenWidth / RetroGame.viewport.AspectRatio, SCREENHEIGHT_MIN, SCREENHEIGHT_MAX);
            this.absoluteCenter = absoluteCenter;
        }

        public override void Initialize()
        {
            zoom = INTRO_INITIAL_ZOOM;
            targetZoom = INTRO_FINAL_ZOOM;
            position = new Vector2(absoluteCenter.X - zoom * (Level.TEX_SIZE / 2) + Level.TILE_SIZE / 2, absoluteCenter.Y - zoom * (Level.TEX_SIZE / 2) - (RetroGame.levelOffsetFromHUD) + Level.TILE_SIZE / 2);
            introFinished = false;
            scrollCamera(absoluteCenter, 100);
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            scrolling = false;
            scrollCamera(seconds);
        }

        public override Vector2 GetRelativeScreenPosition(Entity entityOnScreen)
        {
            Vector2 pos = new Vector2();
            pos.X = (entityOnScreen.position.X - position.X + Level.TILE_SIZE / 2) / (Level.TEX_SIZE * zoom);
            pos.Y = (entityOnScreen.position.Y - position.Y + Level.TILE_SIZE / 2) / ((Level.TEX_SIZE + RetroGame.levelOffsetFromHUD) * zoom);
            return pos;
        }

        private void scrollCamera(float seconds)
        {
            scrollCamera(absoluteCenter, seconds);
        }

        private void scrollCamera(Vector2 destination, float seconds)
        {
            targetPos = new Vector2(destination.X - zoom * (Level.TEX_SIZE / 2) + Level.TILE_SIZE / 2, destination.Y - zoom * (Level.TEX_SIZE / 2) - (RetroGame.levelOffsetFromHUD) + Level.TILE_SIZE / 2);
            if (targetPos.X - position.X > seconds * scrollSpeed)
                position.X += scrollSpeed * seconds;
            else if (targetPos.X - position.X < -seconds * scrollSpeed)
                position.X -= scrollSpeed * seconds;
            else
                position.X = targetPos.X;
            if (targetPos.Y - position.Y > seconds * scrollSpeed)
                position.Y += scrollSpeed * seconds;
            else if (targetPos.Y - position.Y < -seconds * scrollSpeed)
                position.Y -= scrollSpeed * seconds;
            else
                position.Y = targetPos.Y;
            if (position == targetPos)
                scrollMultiplier = 1f;
        }

        public override Matrix GetTranslation()
        {
            return Matrix.CreateTranslation(new Vector3(-position.X + Level.TILE_SIZE / 2, -(position.Y) + Level.TILE_SIZE / 2, 0));
        }

        public override Matrix GetScale()
        {
            return Matrix.CreateScale(new Vector3(RetroGame.viewport.Width / (zoom * Level.TEX_SIZE), RetroGame.viewport.Height / (zoom * (Level.TEX_SIZE + RetroGame.levelOffsetFromHUD)), 1));
        }

        public override Matrix GetViewMatrix()
        {
            return GetTranslation() * GetScale();
        }
    }
}
