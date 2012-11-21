using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class RetroStasis
    {
        // retrostasis values
        public static readonly float RETROSTASIS_DURATION = 5f;
        public static readonly float RETROSTASIS_TIMESCALE = 0.5f;
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ALL = 1f; //secs
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES = RETROSTASIS_INITIAL_FREEZE_TIME_ALL + 0.5f; //secs
        public static readonly float RETROSTASIS_COOLDOWN = 1.5f;
        private static float timeInRetroStasis = 0f;
        private static float effectInnerRadius;
        private static float effectOuterRadius;
        private static float effectIntensity = 2f;
        private static float EFFECT_INNERRADIUS_MAX = 100f;
        private static float EFFECT_FINISHED_RADIUS;
        private static float EFFECT_OUTRO_SPEEDUP_RADIUS;
        private static float effectOutroModifier = 1f;
        private static readonly float effectIntroVelocity = 1800f;
        private static readonly float effectOutroVelocity = 900f;
        private static bool effectFinished = true;
        private static bool cancelRetroStasis = false;
        private static float retroStatisRecharge = 0;

        public static void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(1f);
            float heroTimeScale = 0f;
            if (Game1.retroStatisActive)
            {
                timeInRetroStasis += seconds;
                effectFinished = false;
                EFFECT_FINISHED_RADIUS = Game1.screenSize.Y * 3f * Game1.levelManager.zoom;
                if (effectOuterRadius < EFFECT_FINISHED_RADIUS)
                    effectOuterRadius += effectIntroVelocity * seconds;
                if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL)
                {
                    heroTimeScale = RETROSTASIS_TIMESCALE;
                    effectInnerRadius = (timeInRetroStasis - RETROSTASIS_INITIAL_FREEZE_TIME_ALL) / RETROSTASIS_DURATION * EFFECT_INNERRADIUS_MAX;
                }
                if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES)
                {
                    Game1.timeScale = RETROSTASIS_TIMESCALE;
                }
                Game1.drawEffects = true;
                Game1.currentEffect = Effects.Grayscale;
                Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                Game1.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                Game1.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
                if (timeInRetroStasis >= RETROSTASIS_DURATION)
                {
                    cancelRetroStasis = true;
                }
                if (cancelRetroStasis)
                {
                    Game1.retroStatisActive = false;
                    Game1.timeScale = 1f;
                    timeInRetroStasis = 0f;
                    cancelRetroStasis = false;
                    RiotGuardWall.setReverse(false);
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
                    EFFECT_FINISHED_RADIUS = Game1.screenSize.Y * 3f * Game1.levelManager.zoom;
                    EFFECT_OUTRO_SPEEDUP_RADIUS = Game1.screenSize.Y * Game1.levelManager.zoom;
                    if (effectInnerRadius < EFFECT_FINISHED_RADIUS)
                    {
                        if (effectInnerRadius >= EFFECT_OUTRO_SPEEDUP_RADIUS)
                            effectOutroModifier = 3f;
                        Game1.drawEffects = true;
                        Game1.currentEffect = Effects.Grayscale;
                        effectInnerRadius += effectOutroVelocity * effectOutroModifier *  seconds;
                        Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                        Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                        Game1.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                        Game1.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                        Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                        Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                        Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
                    }
                    else{
                        effectOutroModifier = 1f;
                        effectFinished = true;
                    }
                }
                retroStatisRecharge += seconds;
            }
            Hero.instance.heroTimeScale = heroTimeScale;
        }

        public static float getChargePercentage()
        {
            return retroStatisRecharge / RETROSTASIS_COOLDOWN;
        }

        public static bool canActivate()
        {
            return (retroStatisRecharge >= RETROSTASIS_COOLDOWN) && !Game1.retroStatisActive;
        }

        public static bool canDeactivate()
        {
            return timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL;
        }

        public static void activate()
        {
            Game1.retroStatisActive = true;
            retroStatisRecharge = 0;
            Game1.timeScale = 0;
        }

        public static void deactivate()
        {
            cancelRetroStasis = true;
        }
    }
}
