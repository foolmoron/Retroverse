using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class RetroStasis : Powerup
    {
        public const int SAND_ADDED_ON_COLLECT = 2;

        // retrostasis values
        public static readonly float RETROSTASIS_DURATION = 5f;
        public static readonly float RETROSTASIS_TIMESCALE = 0.5f;
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ALL = 1f; //secs
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES = RETROSTASIS_INITIAL_FREEZE_TIME_ALL + 0.5f; //secs
        public static readonly float RETROSTASIS_COOLDOWN = 1.5f;
        private float timeInRetroStasis = 0f;
        private float effectInnerRadius;
        private float effectOuterRadius;
        private float effectIntensity = 2f;
        private static float EFFECT_INNERRADIUS_MAX = 100f;
        private static float EFFECT_FINISHED_RADIUS;
        private static float EFFECT_OUTRO_SPEEDUP_RADIUS;
        private float effectOutroModifier = 1f;
        private readonly float effectIntroVelocity = 1800f;
        private readonly float effectOutroVelocity = 900f;
        private bool effectFinished = true;
        private bool cancelRetroStasis = false;
        private float retroStatisRecharge = 0;

        public RetroStasis(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Retro";
            SpecificName = "Slowmo";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = true; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("retroicon2"); //filename for this powerup's icon
            DrawBeforeHero = true; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            TintColor = Color.LightGray; //what color should this powerup's icon and related effects be?
            Description = "Slows playback of the\ngame, giving you\ngreater reaction time"; //give a short description (with appropriate newlines) of the powerup, for display to the player
            GemCost = COST_EXPENSIVE; //how many gems does it take to buy this from the store?

            effectInnerRadius = 0;
            effectOuterRadius = 0;
            effectOutroModifier = 1f;
            effectFinished = true;
        }

        public override void OnCollectedByHero(Hero collector)
        {
            for (int i = 0; i < SAND_ADDED_ON_COLLECT; i++)
            {
                RetroGame.AddSand();
            }
        }

        public override void Activate(InputAction activationAction)
        {
            switch (RetroGame.State)
            {
                case GameState.Arena:
                case GameState.Escape:
                    if (RetroGame.AvailableSand > 0 && canActivate())
                    {
                        RetroGame.retroStatisActive = true;
                        retroStatisRecharge = 0;
                        RetroGame.timeScale = 0;
                        RetroGame.RemoveSand();
                    }
                    else
                    {
                        if (canDeactivate())
                        {
                            deactivate();
                        }
                    }
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            LevelManagerScreen topScreen = RetroGame.TopLevelManagerScreen;
            float seconds = gameTime.getSeconds(1f);
            float heroTimeScale = 0f;
            if (!hero.Alive)
            {
                cancelRetroStasis = true;
                return;
            }
            if (RetroGame.retroStatisActive)
            {
                timeInRetroStasis += seconds;
                effectFinished = false;
                EFFECT_FINISHED_RADIUS = RetroGame.screenSize.Y * 3f * topScreen.levelManager.Camera.zoom;
                if (effectOuterRadius < EFFECT_FINISHED_RADIUS)
                    effectOuterRadius += effectIntroVelocity * seconds;
                if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL)
                {
                    heroTimeScale = RETROSTASIS_TIMESCALE;
                    effectInnerRadius = (timeInRetroStasis - RETROSTASIS_INITIAL_FREEZE_TIME_ALL) / RETROSTASIS_DURATION * EFFECT_INNERRADIUS_MAX;
                }
                if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES)
                {
                    RetroGame.timeScale = RETROSTASIS_TIMESCALE;
                }
                topScreen.drawEffects = true;
                topScreen.currentEffect = Effects.Grayscale;
                topScreen.currentEffect.Parameters["width"].SetValue(RetroGame.screenSize.X);
                topScreen.currentEffect.Parameters["height"].SetValue(RetroGame.screenSize.Y);
                topScreen.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                topScreen.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                topScreen.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                topScreen.currentEffect.Parameters["zoom"].SetValue(topScreen.levelManager.Camera.zoom);
                topScreen.currentEffect.Parameters["center"].SetValue(topScreen.levelManager.Camera.GetRelativeScreenPosition(hero));
                if (timeInRetroStasis >= RETROSTASIS_DURATION)
                {
                    cancelRetroStasis = true;
                }
                if (cancelRetroStasis)
                {
                    RetroGame.retroStatisActive = false;
                    RetroGame.timeScale = 1f;
                    timeInRetroStasis = 0f;
                    cancelRetroStasis = false;
                }
            }
            else
            {
                heroTimeScale = 1f;
                if (effectFinished)
                {
                    effectInnerRadius = 0;
                    effectOuterRadius = 0;
                }
                else
                {
                    EFFECT_FINISHED_RADIUS = RetroGame.screenSize.Y * 3f * topScreen.levelManager.Camera.zoom;
                    EFFECT_OUTRO_SPEEDUP_RADIUS = RetroGame.screenSize.Y * topScreen.levelManager.Camera.zoom;
                    if (effectInnerRadius < EFFECT_FINISHED_RADIUS)
                    {
                        if (effectInnerRadius >= EFFECT_OUTRO_SPEEDUP_RADIUS)
                            effectOutroModifier = 3f;
                        topScreen.drawEffects = true;
                        topScreen.currentEffect = Effects.Grayscale;
                        effectInnerRadius += effectOutroVelocity * effectOutroModifier *  seconds;
                        topScreen.currentEffect.Parameters["width"].SetValue(RetroGame.screenSize.X);
                        topScreen.currentEffect.Parameters["height"].SetValue(RetroGame.screenSize.Y);
                        topScreen.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                        topScreen.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                        topScreen.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                        topScreen.currentEffect.Parameters["zoom"].SetValue(topScreen.levelManager.Camera.zoom);
                        topScreen.currentEffect.Parameters["center"].SetValue(topScreen.levelManager.Camera.GetRelativeScreenPosition(hero));
                    }
                    else{
                        effectOutroModifier = 1f;
                        effectFinished = true;
                    }
                }
                retroStatisRecharge += seconds;
            }
            Hero.HERO_TIMESCALE = heroTimeScale;
        }

        public override float  GetPowerupCharge()
        {
            if(RetroGame.AvailableSand > 0)
                return retroStatisRecharge / RETROSTASIS_COOLDOWN;
            return 0;
        }

        public bool canActivate()
        {
            return (retroStatisRecharge >= RETROSTASIS_COOLDOWN) && !RetroGame.retroStatisActive;
        }

        public bool canDeactivate()
        {
            return timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL;
        }

        public void deactivate()
        {
            cancelRetroStasis = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {            
        }
    }
}
