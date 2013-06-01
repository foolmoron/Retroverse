using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class CoopEscapeCamera : EscapeCamera
    {
        public const float MAX_DISTANCE_BETWEEN_ENTITIES_DEFAULT = 500f;
        public float MaxDistanceBetweenEntities { get; set; }

        public Entity otherEntity;

        public CoopEscapeCamera(Entity targetEntity, Entity otherEntity) : base(targetEntity)
        {
            this.otherEntity = otherEntity;

            MaxDistanceBetweenEntities = MAX_DISTANCE_BETWEEN_ENTITIES_DEFAULT;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            Vector2 scrollTarget;

            if (Vector2.Distance(targetEntity.position, otherEntity.position) <= MaxDistanceBetweenEntities)
            {
                Vector2 centerBetween = Vector2.Lerp(targetEntity.position, otherEntity.position, 0.5f);
                scrollTarget = centerBetween;
            }
            else
            {
                Vector2 unitVectorBetween = (otherEntity.position - targetEntity.position);
                unitVectorBetween.Normalize();
                Vector2 maxDistanceFromTargetEntityTowardsOther = targetEntity.position + (unitVectorBetween * (MaxDistanceBetweenEntities / 2));
                scrollTarget = maxDistanceFromTargetEntityTowardsOther;
            }
            scrollCameraToTarget(scrollTarget, seconds);
        }
    }
}
