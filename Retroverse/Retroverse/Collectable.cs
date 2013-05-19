using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public abstract class Collectable : Entity
    {
        public string CollectedSound { get; protected set; }
        protected enum CollectedAction { Delete, Regenerate }
        protected CollectedAction ActionAfterCollected { get; set; }
        public double collectedTime;
        public float timeAlive = 0;
        public bool ableToBeCollected = true;
        public bool addsToProgress = true;
        public bool dying = false;
        public Entity collectedByEntity = null;
        public Emitter emitter;
        public static readonly float COLLECTABLE_SCORE_MAXIMUM_RAMP_UP_TIME = 10f; //secs
        public static readonly float COLLECTABLE_SCORE_RANDOM_BONUS_PERCENTAGE = 0.1f;
        public static readonly int COLLECTABLE_SCORE = 400;
        protected int baseScore = COLLECTABLE_SCORE;
        public float rampUpScoreBonus = 0;
        
        public Collectable(int x,int y, int levelX, int levelY, int tileX, int tileY)
            : base(new Vector2(x, y), new Hitbox(32, 32))
        {
            CollectedSound = "CollectGem";
            ActionAfterCollected = CollectedAction.Delete;
            this.setTexture("collectable3");
            this.levelX = levelX;
            this.levelY = levelY;
            this.tileX = tileX;
            this.tileY = tileY;
            emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.CollectedSparks);
            emitter.position = position;
            scale = 0.5f;
        }

        public override void Update(GameTime gameTime)
        {
            timeAlive += gameTime.getSeconds();
            float rampUpFactor = (timeAlive / COLLECTABLE_SCORE_MAXIMUM_RAMP_UP_TIME);
            if (rampUpFactor > 1f)
                rampUpFactor = 1;
            rampUpScoreBonus = rampUpFactor * (baseScore / 4f);
            if (dying)
            {
                emitter.position = position;
                emitter.Update(gameTime);
                if (emitter.isFinished())
                {
                    if (ActionAfterCollected == CollectedAction.Delete)
                        RetroGame.EscapeScreen.levelManager.collectablesToRemove.Add(this);
                    else if (ActionAfterCollected == CollectedAction.Regenerate)
                    {
                        dying = false;
                        timeAlive = 0;
                        ableToBeCollected = true;
                        collectedByEntity = null;
                        emitter.Reset();
                        emitter.active = true;
                        rampUpScoreBonus = 0;
                    }
                }
            }
            updateCurrentLevelAndTile();
            base.Update(gameTime);
        }

        public virtual bool collectedBy(Entity e)
        {
            if (!dying && ableToBeCollected)
            {
                collectedTime = latestGameTime.TotalGameTime.TotalMilliseconds;
                float randomScoreBonus = ((float)RetroGame.rand.NextDouble() - 0.5f) * (baseScore * COLLECTABLE_SCORE_RANDOM_BONUS_PERCENTAGE);
                if (baseScore > 0)
                    RetroGame.AddScore((int)(baseScore + rampUpScoreBonus + randomScoreBonus));
                dying = true;
                ableToBeCollected = false;
                collectedByEntity = e;
                if (!string.IsNullOrEmpty(CollectedSound))
                    SoundManager.PlaySoundOnce(CollectedSound);
                return true;
            }
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (dying)
                emitter.Draw(spriteBatch);
            else
                base.Draw(spriteBatch);
        }
    }
}
