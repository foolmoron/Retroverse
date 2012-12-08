using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class Collectable : Entity
    {
        public static readonly float MOVE_SPEED = 760f;
        public int levelX, levelY, tileX, tileY;
        public double collectedTime;
        public float timeAlive = 0;
        public bool ableToBeCollected = true;
        public bool addsToProgress = true;
        public bool dying = false;
        public bool collectedThisFrame = false;
        public Emitter emitter;
        public static readonly float COLLECTABLE_SCORE_MAXIMUM_RAMP_UP_TIME = 10f; //secs
        public static readonly float COLLECTABLE_SCORE_RANDOM_BONUS_PERCENTAGE = 0.1f;
        public static readonly int COLLECTABLE_SCORE = 400;
        protected int baseScore = COLLECTABLE_SCORE;
        public float rampUpScoreBonus = 0;
        
        public Collectable(int x,int y, int levelX, int levelY, int tileX, int tileY)
            : base(new Hitbox(32, 32))
        {
            position = new Vector2(x, y);
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
            collectedThisFrame = false;
            timeAlive += gameTime.getSeconds();
            float rampUpFactor = (timeAlive / COLLECTABLE_SCORE_MAXIMUM_RAMP_UP_TIME);
            if (rampUpFactor > 1f)
                rampUpFactor = 1;
            rampUpScoreBonus = rampUpFactor * (baseScore / 4);
            if (dying)
            {
                emitter.position = position;
                emitter.Update(gameTime);
                if (emitter.isFinished())
                {
                    Game1.levelManager.collectablesToRemove.Add(this);
                }
            }
            else
            {
                if (Hero.instance.hitbox.intersects(hitbox) && ableToBeCollected)
                {
                    collectedTime = gameTime.TotalGameTime.TotalMilliseconds;
                    float randomScoreBonus = ((float)Game1.rand.NextDouble() - 0.5f) * (baseScore * COLLECTABLE_SCORE_RANDOM_BONUS_PERCENTAGE);
                    if (baseScore > 0)
                        Game1.addScore((int)(baseScore + rampUpScoreBonus + randomScoreBonus));
                    dying = true;
                    collectedThisFrame = true;
                    if (addsToProgress && Game1.state == GameState.Arena)
                        Powerups.addToProgress(this);
                }
            }
            hitbox.Update(this);
            base.Update(gameTime);
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
