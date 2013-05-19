using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

/* Start with copying the PowerupTEMPLATE over */
namespace Retroverse
{
    public class BlinkPowerup : Powerup, IReversible
    {
        /* Put whatever variables and fields are needed in the class */
        public float BLINK_INTERVAL = 10f; //secs
        public float BLINK_MISS_PENALTY = 1.5f; //secs
        public float blinkTimer = 0f;
        public const float BLINK_DISTANCE = 150f;
        public const int FORGIVENESS = 5;

        private Vector2 positionToMoveToOnBlink;

        Emitter blinkEmitter, endEmitter; // we want there to be a blast when you teleport and another when you land so we need two particle emitters

        public BlinkPowerup(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Blink";
            SpecificName = "Blink";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("blinkicon"); //placeholder icon
            DrawBeforeHero = false; //draw the particles above the hero
            GemCost = COST_VERYEXPENSIVE; //how many gems does it take to buy this from the store?
            TintColor = Color.Aquamarine; //what color should this powerup's icon and related effects be?
            Description = "Instantly teleports a\nshort distance forward"; //give a short description (with appropriate newlines) of the powerup, for display to the player

            /* Do any other sort of initialization right here */
            initializeBlinkEmitter();
            initializeEndEmitter();
        }

        private void initializeBlinkEmitter()
        {
            blinkEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.BlinkOriginSparks);
            //blinkEmitter.startColor = Color.Red;
            //blinkEmitter.endColor = Color.Purple;
            blinkEmitter.active = false;
        }

        private void initializeEndEmitter()
        {
            endEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.BlinkEndSparks);
            //endEmitter.startColor = Color.Purple;
            //endEmitter.endColor = Color.Red;
            endEmitter.active = false;
        }

        public override void Activate(InputAction activationAction)
        {
            positionToMoveToOnBlink = hero.position;
            if (blinkTimer >= BLINK_INTERVAL && canBlinkSuccessfully())
            {
                blinkTimer = 0;
                blinkEmitter.active = true;

                hero.position = positionToMoveToOnBlink;
                hero.teleportedThisFrame = true;
                
                endEmitter.position = hero.position;
                endEmitter.active = true;
                SoundManager.PlaySoundOnce("Blink", playInReverseDuringReverse: true);
            }
        }

        private bool canBlinkSuccessfully()
        {
            Direction dir = hero.direction;
            Vector2 dirVector = dir.toVector();
            Vector2 movement = dirVector * BLINK_DISTANCE;

            Vector2 attemptedDestination = hero.position + movement;

            const int STEP = Level.TILE_SIZE;
            Vector2 difference = attemptedDestination - hero.position;
            float mostExtremeMovementValue = Math.Max(Math.Abs(difference.X), Math.Abs(difference.Y));
            int stepsToDestination = (int)(mostExtremeMovementValue / STEP) + 1;
            bool wallEncountered = false;
            bool blinkSuccessful = false;
            Vector2 currentPos = hero.position;
            for (int i = 0; i < (stepsToDestination + 1); i++, currentPos += (dirVector * STEP))
            {
                int levelX = (int)(currentPos.X / Level.TEX_SIZE); // get which level you are in
                int levelY = (int)(currentPos.Y / Level.TEX_SIZE);
                int tileX = (int)((currentPos.X % Level.TEX_SIZE) / Level.TILE_SIZE); // get which tile you are moving to
                int tileY = (int)((currentPos.Y % Level.TEX_SIZE) / Level.TILE_SIZE);
                if (RetroGame.TopLevelManagerScreen.levelManager.levels[levelX, levelY] == null)
                    return false; //quit with failure if level map is broken or empty (such as in the store)
                if (RetroGame.TopLevelManagerScreen.levelManager.levels[levelX, levelY].grid[tileX, tileY] != LevelContent.LevelTile.Wall)
                    continue; //keep going if no wall
                wallEncountered = true;
                Vector2 nextPos = currentPos + (dirVector * STEP);
                int nextLevelX = (int)(nextPos.X / Level.TEX_SIZE);
                int nextLevelY = (int)(nextPos.Y / Level.TEX_SIZE);
                int nextTileX = (int)((nextPos.X % Level.TEX_SIZE) / Level.TILE_SIZE);
                int nextTileY = (int)((nextPos.Y % Level.TEX_SIZE) / Level.TILE_SIZE);
                if (RetroGame.TopLevelManagerScreen.levelManager.levels[nextLevelX, nextLevelY] != null)
                    if (RetroGame.TopLevelManagerScreen.levelManager.levels[nextLevelX, nextLevelY].grid[nextTileX, nextTileY] != LevelContent.LevelTile.Wall)
                    {
                        positionToMoveToOnBlink = new Vector2(nextLevelX * Level.TEX_SIZE + nextTileX * Level.TILE_SIZE + Level.TILE_SIZE / 2, nextLevelY * Level.TEX_SIZE + nextTileY * Level.TILE_SIZE + Level.TILE_SIZE / 2);
                        blinkSuccessful = true;
                    }
                break; //once we have checked the tile after the first wall, we are done
            }
            if (!wallEncountered)
            {
                positionToMoveToOnBlink = attemptedDestination;
                blinkSuccessful = true;
            }
            return blinkSuccessful;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(Hero.HERO_TIMESCALE);
            blinkTimer += seconds * hero.powerupCooldownModifier; //keeps track of cooldown

            if (blinkEmitter.active)
            {
                blinkEmitter.Update(gameTime);
                if (blinkEmitter.isFinished())
                    blinkEmitter.Reset();
            }
            else
            {
                blinkEmitter.position = hero.position;
            }

            if (endEmitter.active)
            {
                endEmitter.Update(gameTime);
                if (endEmitter.isFinished())
                    endEmitter.Reset();
            }
            else
            {
                endEmitter.position = hero.position;
            }
        }

        public override float GetPowerupCharge()
        {
            float charge = 0;
            charge = blinkTimer / (BLINK_INTERVAL); //charge indicates when you can blink
            if(charge >= 1)
                charge = (canBlinkSuccessfully()) ? 1 : 0;
            return charge;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (endEmitter.active)
                endEmitter.Draw(spriteBatch);
            if (blinkEmitter.active)
                blinkEmitter.Draw(spriteBatch); //just draw the emitter           
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new BlinkMemento(this); //generate new memento using current state of this object
        }

        //this class does not need to be accessible anywhere else, it does all its work here
        private class BlinkMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            IMemento blinkEmitterMemento;
            IMemento endEmitterMemento;

            public BlinkMemento(BlinkPowerup target)
            {
                //save necessary information from target here
                Target = target;
                blinkEmitterMemento = target.blinkEmitter.GenerateMementoFromCurrentFrame();
                endEmitterMemento = target.endEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                BlinkPowerup target = (BlinkPowerup)Target;

                if (nextFrame != null)
                {
                    blinkEmitterMemento.Apply(interpolationFactor, isNewFrame, ((BlinkMemento)nextFrame).blinkEmitterMemento);
                    endEmitterMemento.Apply(interpolationFactor, isNewFrame, ((BlinkMemento)nextFrame).endEmitterMemento);
                }
                else
                {
                    blinkEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                    endEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
            }
        }
    }
}