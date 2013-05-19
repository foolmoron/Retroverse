using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class HeroChaseAI : TargetedAI
    {
        public const float ITERATION_TIME = 1f;
        public float iterationTime = 0f;

        public bool chaseLastKnownTargetPosition;
        public int lastGoodTargetX, lastGoodTargetY;
        public Entity oldTarget;

        Direction oldDirection;

        public HeroChaseAI(bool chaseLastKnownTargetPosition)
        {
            this.chaseLastKnownTargetPosition = chaseLastKnownTargetPosition;
        }

        public override void OnTargetChanged()
        {
        }

        public override void Update(GameTime gameTime)
        {
            iterationTime += gameTime.getSeconds();
        }

        public override Direction GetNextDirection(Entity subject)
        {
            if (Target == null || subject == null)
            {
                return Direction.None;
            }
            else if (!(subject is Enemy))
            {
                Console.WriteLine("Chase AI: not enemy subject");
                return Direction.Invalid;
            }

            Direction nextDir = oldDirection;
            if (subject.tileChanged || Target.tileChanged || iterationTime >= ITERATION_TIME || oldDirection == Direction.Invalid)
            {
                iterationTime = 0;
                
                Level subjectLevel = ((Enemy)subject).level;
                if (chaseLastKnownTargetPosition)
                {
                    if (oldTarget != null)
                    {
                        if (subjectLevel.xPos == oldTarget.levelX || subjectLevel.yPos == oldTarget.levelY)
                        {
                            SetTarget(oldTarget);
                            oldTarget = null;
                        }
                    }   
                }
                if (subjectLevel.xPos != Target.levelX || subjectLevel.yPos != Target.levelY)
                {
                    if (chaseLastKnownTargetPosition)
                    {
                        oldTarget = Target;
                        Entity oldPositionDummy = new Entity(Vector2.Zero);
                        oldPositionDummy.tileX = lastGoodTargetX;
                        oldPositionDummy.tileY = lastGoodTargetY;
                        SetTarget(oldPositionDummy);
                    }
                    else
                    {
                        SetTarget(null);
                        return Direction.None;
                    }
                }

                Point subjectPosition = new Point(subject.tileX, subject.tileY);
                nextDir = subjectLevel.pathfinding.GetNextDirection(subjectPosition, new Point(Target.tileX, Target.tileY));
                if (nextDir == Direction.Invalid)
                    Console.WriteLine("Chase AI: nextDir invalid");
                lastGoodTargetX = Target.tileX;
                lastGoodTargetY = Target.tileY;
            }
            oldDirection = nextDir;
            return oldDirection;
        }

        public override float GetNextMoveSpeedMultiplier(Entity subject)
        {
            return 1;
        }
    }
}
