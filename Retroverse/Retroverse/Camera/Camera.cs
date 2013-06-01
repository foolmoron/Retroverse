using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public abstract class Camera
    {
        public float zoom;
        public float targetZoom;
        public float zoomSpeed = 0.25f;

        public Vector2 position;
        public Vector2 targetPos;
        public Vector2 acceleration;
        public Vector2 absoluteCenter;

        public abstract void Initialize();

        public void InitializeWithCamera(Camera otherCamera)
        {
            zoom = otherCamera.zoom;
            position = otherCamera.position;
        }

        public abstract void Update(GameTime gameTime);
        
        public abstract Matrix GetTranslation();
        public abstract Matrix GetScale();
        public abstract Matrix GetViewMatrix();

        public abstract Vector2 GetRelativeScreenPosition(Entity entityOnScreen);
    }
}
